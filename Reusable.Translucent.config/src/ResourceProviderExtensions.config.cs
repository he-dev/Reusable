using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Quickey;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class ResourceProviderExtensions
    {
        public static async Task<object> ReadSettingAsync(this IResourceRepository resourceRepository, Selector selector)
        {
            var request = ConfigRequestBuilder.CreateRequest(RequestMethod.Get, selector);
            using (var response = await resourceRepository.InvokeAsync(request))
            {
                return
                    response
                        .Metadata
                        .GetItem(ConfigController.Converter)
                        .Convert(response.Body, selector.DataType);
            }
        }

        public static async Task WriteSettingAsync(this IResourceRepository resourceRepository, Selector selector, object newValue)
        {
            using (var request = ConfigRequestBuilder.CreateRequest(RequestMethod.Put, selector, newValue))
            {
                await resourceRepository.InvokeAsync(request);
            }
        }

        #region Helpers

        private static object Validate(object value, IEnumerable<ValidationAttribute> validations, UriString uri)
        {
            foreach (var validation in validations)
            {
                validation.Validate(value, uri);
            }

            return value;
        }

        #endregion

        #region Getters

        public static async Task<T> ReadSettingAsync<T>(this IResourceRepository resourceRepository, Selector<T> selector)
        {
            return (T)await resourceRepository.ReadSettingAsync((Selector)selector);
        }

        public static T ReadSetting<T>(this IResourceRepository resourceRepository, Selector<T> selector)
        {
            return (T)resourceRepository.ReadSettingAsync((Selector)selector).GetAwaiter().GetResult();
        }

        public static async Task<T> ReadSettingAsync<T>(this IResourceRepository resourceRepository, Expression<Func<T>> selector, string index = default)
        {
            return (T)await resourceRepository.ReadSettingAsync(CreateSelector<T>(selector, index));
        }

        public static T ReadSetting<T>(this IResourceRepository resourceRepository, [NotNull] Expression<Func<T>> selector, string index = default)
        {
            return ReadSettingAsync(resourceRepository, selector, index).GetAwaiter().GetResult();
        }

        #endregion

        #region Setters

        public static async Task WriteSettingAsync<TValue>(this IResourceRepository resourceRepository, Expression<Func<TValue>> selector, TValue newValue, string index = default)
        {
            await resourceRepository.WriteSettingAsync(CreateSelector<TValue>(selector, index), newValue);
        }

        public static void WriteSetting<T>(this IResourceRepository resourceRepository, [NotNull] Expression<Func<T>> selector, [CanBeNull] T newValue, string index = default)
        {
            resourceRepository.WriteSettingAsync(selector, newValue, index).GetAwaiter().GetResult();
        }

        #endregion

        private static Selector CreateSelector<T>(LambdaExpression selector, string index)
        {
            return
                index is null
                    ? new Selector<T>(selector)
                    : new Selector<T>(selector).Index(index);
        }
    }
}