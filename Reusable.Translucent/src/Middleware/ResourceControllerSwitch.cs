using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent.Middleware
{
    /// <summary>
    /// Handles requests and determines which resource-controller to use to get a resources. Controllers determined with a GET requests are cached.
    /// </summary>
    [UsedImplicitly]
    public class ResourceControllerSwitch : MiddlewareBase
    {
        private static readonly IEnumerable<ResourceControllerFilterDelegate> Filters = new ResourceControllerFilterDelegate[]
        {
            ResourceControllerFilters.FilterByControllerName,
            ResourceControllerFilters.FilterByRequest,
            //ResourceControllerFilters.FilterByUriPath,
        };

        private readonly IImmutableList<IController> controllers;
        private readonly ILogger? logger;
        private readonly IMemoryCache cache;

        public ResourceControllerSwitch
        (
            RequestDelegate<ResourceContext> next,
            ILogger<ResourceControllerSwitch> logger,
            IMemoryCache cache,
            IEnumerable<IController> controllers
        ) : base(next)
        {
            this.logger = logger;
            this.controllers = controllers.ToImmutableList();
            this.cache = cache;
        }

        public override async Task InvokeAsync(ResourceContext context)
        {
            //await InvokeNext(context);

            var providerKey = context.Request.ResourceName;

            // Used cached provider if already resolved.
            if (cache.TryGetValue<IController>(providerKey, out var entry))
            {
                context.Response = await InvokeMethodAsync(entry, context.Request);
            }
            else
            {
                var candidates = Filters.Aggregate(controllers.AsEnumerable(), (providers, filter) => filter(providers, context.Request));

                // GET can search multiple providers.
                if (context.Request.Method == ResourceMethod.Get)
                {
                    context.Response = Response.NotFound();
                    foreach (var controller in candidates)
                    {
                        if (await InvokeMethodAsync(controller, context.Request) is var response && response.Exists())
                        {
                            context.Response = response;
                            cache.Set(providerKey, controller);
                            logger?.Log(Abstraction.Layer.IO().Meta(new
                            {
                                resource = new
                                {
                                    controller = controller.GetType().ToPrettyString(),
                                    name = controller.Name,
                                    statusCode = ResourceStatusCode.OK
                                }
                            }));
                            break;
                        }
                        else
                        {
                            logger?.Log(Abstraction.Layer.IO().Meta(new
                            {
                                resource = new
                                {
                                    controller = controller.GetType().ToPrettyString(),
                                    name = controller.Name,
                                    statusCode = ResourceStatusCode.NotFound
                                }
                            }));
                        }
                    }
                }
                // Other methods are allowed to use only a single controller.
                else
                {
                    var controller = cache.Set(providerKey, candidates.SingleOrThrow(onEmpty: ($"{nameof(Controller)}NotFound", $"Could not find controller for resource '{context.Request.ResourceName}'.")));
                    context.Response = await InvokeMethodAsync(controller, context.Request);
                }
            }
        }

        private static Task<Response> InvokeMethodAsync(IController controller, Request request)
        {
            var methods =
                controller
                    .GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => m.GetCustomAttribute<ResourceActionAttribute>()?.Method.Equals(request.Method) == true);

            var method = methods.SingleOrThrow
            (
                onEmpty: ("MethodNotFound", $"Could not find method '{request.Method}' on controller '{controller.GetType().ToPrettyString()}'"),
                onMany: ("AmbiguousMethod", $"There is more than one method '{request.Method}' on controller '{controller.GetType().ToPrettyString()}'")
            );

            var requestType = method.GetParameters().Single().ParameterType;
            
            return (Task<Response>)method.Invoke(controller, new object[] { request });
        }
    }
}