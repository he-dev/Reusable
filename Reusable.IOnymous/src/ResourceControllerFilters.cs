using System.Collections.Generic;
using System.Linq;
using Reusable.Data;

namespace Reusable.IOnymous
{
    public delegate IEnumerable<IResourceController> ResourceProviderFilterCallback(IEnumerable<IResourceController> providers, Request request);

    public static class ResourceControllerFilters
    {
        public static IEnumerable<IResourceController> FilterByProviderTags(this IEnumerable<IResourceController> providers, Request request)
        {
            if (!request.Metadata.GetItemOrDefault(ResourceControllerProperties.Tags).Any())
            {
                return providers;
            }
            
            return
                from p in providers
                let providerTags = p.Properties.GetItemOrDefault(ResourceControllerProperties.Tags)
                where providerTags.Overlaps(request.Metadata.GetItemOrDefault(ResourceControllerProperties.Tags))
                select p;
        }

        public static IEnumerable<IResourceController> FilterByUriScheme(this IEnumerable<IResourceController> providers, Request request)
        {
            var canFilter = !(request.Uri.IsRelative || (request.Uri.IsAbsolute && request.Uri.Scheme == UriSchemes.Custom.IOnymous));
            return
                from p in providers
                let schemes = p.Properties.GetItem(ResourceControllerProperties.Schemes)
                where !canFilter || schemes.Overlaps(new[] { UriSchemes.Custom.IOnymous, request.Uri.Scheme })
                select p;
        }

        public static IEnumerable<IResourceController> FilterByUriPath(this IEnumerable<IResourceController> providers, Request request)
        {
            if (request.Uri.IsAbsolute)
            {
                return providers;
            }

            return
                from p in providers
                where p.SupportsRelativeUri()
                select p;
        }

//        public static IEnumerable<IResourceProvider> FilterByScheme(this IEnumerable<IResourceProvider> providers, Request request)
//        {
//            var canFilter = !(request.Uri.IsRelative || (request.Uri.IsAbsolute && request.Uri.Scheme == UriSchemes.Custom.IOnymous));
//            return providers.Where(p => !canFilter || p.Properties.GetSchemes().Overlaps(new[] { UriSchemes.Custom.IOnymous, request.Uri.Scheme }));
//        }
    }
}