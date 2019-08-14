using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Exceptionize;

namespace Reusable.IOnymous.Middleware
{
    [UsedImplicitly]
    public class ControllerMiddleware
    {
        private static readonly IEnumerable<ResourceProviderFilterCallback> Filters = new ResourceProviderFilterCallback[]
        {
            ResourceControllerFilters.FilterByUriScheme,
            ResourceControllerFilters.FilterByUriPath,
            ResourceControllerFilters.FilterByProviderTags,
        };

        private readonly IImmutableList<IResourceController> _controllers;
        private readonly RequestCallback<ResourceContext> _next;
        private readonly IMemoryCache _cache;

        public ControllerMiddleware(RequestCallback<ResourceContext> next, IEnumerable<IResourceController> controllers)
        {
            _next = next;
            _controllers = controllers.ToImmutableList();
            _cache = new MemoryCache(new MemoryCacheOptions { });
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            var providerKey = context.Request.Uri.ToString();

            // Used cached provider if already resolved.
            if (_cache.TryGetValue<IResourceController>(providerKey, out var entry))
            {
                context.Response = await InvokeAsync(entry, context.Request);
            }
            else
            {
                var filtered = Filters.Aggregate(_controllers.AsEnumerable(), (providers, filter) => filter(providers, context.Request));

                // GET can search multiple providers.
                if (context.Request.Method == RequestMethod.Get)
                {
                    context.Response = Resource.DoesNotExist.FromRequest(context.Request);
                    foreach (var provider in filtered)
                    {
                        if (await InvokeAsync(provider, context.Request) is var resource && resource.Exists)
                        {
                            _cache.Set(providerKey, provider);
                            context.Response = resource;
                            break;
                        }
                    }
                }
                // Other methods are allowed to use only a single provider.
                else
                {
                    var resourceProvider =
                        _cache.Set(
                            providerKey,
                            filtered.SingleOrThrow(
                                onEmpty: () => DynamicException.Create("ResourceProviderNotFound", $"Could not get resource '{context.Request.Uri.ToString()}'.")));
                    context.Response = await InvokeAsync(resourceProvider, context.Request);
                }
            }

            await _next(context);
            //context.Response.Uri = 
        }

        private Task<IResource> InvokeAsync(IResourceController controller, Request request)
        {
            var method =
                controller
                    .GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .SingleOrDefault(m => m.GetCustomAttribute<ResourceActionAttribute>()?.Method == request.Method);

            return (Task<IResource>)method.Invoke(controller, new object[] { request });
        }


        // crud
    }

//    {
//    if (Methods is null)
//    {
//        throw new InvalidOperationException
//        (
//            $"{nameof(Methods)} property is not initialized. " +
//            $"You must specify at least one method by initializing this property in the derived type."
//        );
//    }
//
//    if (request.Method == RequestMethod.None)
//    {
//        throw new ArgumentException(paramName: nameof(request), message: $"You must specify a request method. '{RequestMethod.None}' is not one of them.");
//    }
//
//    if (Methods.TryGetMethod(request.Method, out var method))
//    {
//        try
//        {
//            return new ResourceExceptionHandler(await method(request));
//        }
//        catch (Exception inner)
//        {
//            throw DynamicException.Create
//            (
//                $"Request",
//                $"An error occured in {ResourceProviderHelper.FormatNames(this)} " +
//                $"while trying to {RequestHelper.FormatMethodName(request)} '{request.Uri}'. See the inner exception for details.",
//                inner
//            );
//        }
//    }
//
//    throw DynamicException.Create
//    (
//    $"MethodNotSupported",
//    $"{ResourceProviderHelper.FormatNames(this)} " +
//    $"cannot {RequestHelper.FormatMethodName(request)} '{request.Uri}' because it doesn't support it."
//    );
//    }
}