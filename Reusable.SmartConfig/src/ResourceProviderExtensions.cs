using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig
{
    public static class ResourceProviderExtensions
    {
        #region GetValue overloads

        [ItemNotNull]
        public static async Task<T> GetSettingAsync<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return (T)await resourceProvider.GetSettingAsync((LambdaExpression)expression, instanceName);
        }

        [ItemNotNull]
        public static T GetSetting<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return (T)resourceProvider.GetSettingAsync((LambdaExpression)expression, instanceName).GetAwaiter().GetResult();
        }

        [ItemNotNull]
        private static async Task<object> GetSettingAsync([NotNull] this IResourceProvider resourceProvider, [NotNull] LambdaExpression expression, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            //var settingName =
            //    settingMetadata
            //        .ToUri()
            //        .CreateSettingName(instanceName)
            //        .ModifySettingName
            //        (
            //            settingMetadata.Strength,
            //            settingMetadata.Prefix,
            //            settingMetadata.PrefixHandling
            //        );

            var settingInfo =
                await
                    resourceProvider
                        .GetAsync(settingMetadata.ToUri(instanceName));

            if (settingInfo.Exists)
            {
                var value = (await settingInfo.DeserializeAsync(settingMetadata.MemberType)) ?? settingMetadata.DefaultValue;
                return value;
                //settingMetadata
                //    .Validations
                //    .Validate(settingName, value);
            }

            return default;
        }

        #endregion

        #region SetValue overloads

        [ItemNotNull]
        public static async Task<IResourceProvider> SetSettingAsync<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);

            //var settingName =
            //    settingMetadata
            //        .CreateSettingName(instanceName)
            //        .ModifySettingName
            //        (
            //            settingMetadata.Strength,
            //            settingMetadata.Prefix,
            //            settingMetadata.PrefixHandling
            //        );

            var settingInfo =
                await
                    resourceProvider
                        .GetAsync(settingMetadata.ToUri(instanceName));

            if (settingInfo.Exists)
            {
                //settingMetadata
                //    .Validations
                //    .Validate(settingName, newValue);

                await resourceProvider.PutAsync(settingMetadata.ToUri(instanceName), newValue);
            }

            return resourceProvider;
        }

        [ItemNotNull]
        public static IResourceProvider SetSetting<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string instanceName = null)
        {
            return resourceProvider.SetSettingAsync(expression, newValue, instanceName).GetAwaiter().GetResult();
        }

        #endregion

        #region AssignValue overloads

        /// <summary>
        /// Assigns the same setting value to the specified member.
        /// </summary>
        [ItemNotNull]
        public static async Task<IResourceProvider> BindSettingAsync<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            //var settingName =
            //    settingMetadata
            //        .CreateSettingName(instanceName)
            //        .ModifySettingName
            //        (
            //            settingMetadata.Strength,
            //            settingMetadata.Prefix,
            //            settingMetadata.PrefixHandling
            //        );

            var value = await resourceProvider.GetSettingAsync(expression, instanceName);

            //settingMetadata
            //    .Validations
            //    .Validate(settingName, value);

            settingMetadata.SetValue(value);

            return resourceProvider;
        }

        /// <summary>
        /// Assigns setting values to all members decorated with the the SmartSettingAttribute.
        /// </summary>
        [ItemNotNull]
        public static async Task<IResourceProvider> BindSettingsAsync<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] T obj, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var settingProperties =
                typeof(T)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.IsDefined(typeof(SettingMemberAttribute)));

            foreach (var property in settingProperties)
            {
                // This expression allows to reuse GeAsync.
                var expression = Expression.Lambda(
                    Expression.Property(
                        Expression.Constant(obj),
                        property.Name
                    )
                );

                var value = await resourceProvider.GetSettingAsync(expression, instanceName);
                var settingMetadata = SettingMetadata.FromExpression(expression, false);
                //var settingName =
                //    settingMetadata
                //        .CreateSettingName(instanceName)
                //        .ModifySettingName
                //        (
                //            settingMetadata.Strength,
                //            settingMetadata.Prefix,
                //            settingMetadata.PrefixHandling
                //        );
                //settingMetadata
                //    .Validations
                //    .Validate(settingName, value);
                settingMetadata.SetValue(value);

            }

            return resourceProvider;
        }

        #endregion

        private static object Validate(this IEnumerable<ValidationAttribute> validations, SettingName settingName, object value)
        {
            foreach (var validation in validations)
            {
                validation.Validate(value, $"Setting {settingName.ToString().QuoteWith("'")} is not valid.");
            }

            return value;
        }
    }
}