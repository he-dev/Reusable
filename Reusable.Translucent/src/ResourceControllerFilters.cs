using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public delegate IEnumerable<IResourceController> ResourceControllerFilterCallback(IEnumerable<IResourceController> controllers, Request request);

    public static class ResourceControllerFilters
    {
        public static IEnumerable<IResourceController> FilterByControllerName(this IEnumerable<IResourceController> controllers, Request request)
        {
            if (request.ControllerName is {})
            {
                return
                    from c in controllers
                    where c.Name.Equals(request.ControllerName)
                    select c;
            }

            return controllers;
        }

        public static IEnumerable<IResourceController> FilterByRequest(this IEnumerable<IResourceController> controllers, Request request)
        {
            return
                from c in controllers
                where c.GetType().GetCustomAttribute<HandlesAttribute>().Type.IsInstanceOfType(request)
                select c;
        }

        public static IEnumerable<IResourceController> FilterByUriPath(this IEnumerable<IResourceController> controllers, Request request)
        {
            if (request.Uri.IsAbsolute)
            {
                return controllers;
            }

            return
                from c in controllers
                where c.SupportsRelativeUri
                select c;
        }
    }
}