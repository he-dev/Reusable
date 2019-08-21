using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class ConfigRequestBuilder
    {
        public static Request CreateRequest(RequestMethod method, Selector selector, object value = default)
        {
            var resources =
                from m in selector.Member.Path()
                where m.IsDefined(typeof(SettingAttribute))
                select m.GetCustomAttribute<SettingAttribute>();

            var resource = resources.FirstOrDefault();

            var uri = UriStringHelper.CreateQuery
            (
                scheme: ConfigController.Scheme,
                path: new[] { "settings" },
                query: new (string Key, string Value)[] { (ConfigController.ResourceNameQueryKey, selector.ToString()) }
            );

            return new Request
            {
                Uri = uri,
                Method = method,
                Metadata =
                    ImmutableContainer
                        .Empty
                        .UpdateItem(ResourceController.Tags, x => resource is null ? x : x.Add(resource.Controller.ToSoftString())),
                Body = value
            };
        }
    }

    public class ConfigRequest : Request
    {
        #region Properties

        private static readonly From<ConfigRequest> This;

        

        #endregion
    }
}