using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters.Generic;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig
{
    public static class SettingProviderExtensions
    {
        #region GetValue overloads

        [ItemNotNull]
        public static async Task<T> GetSettingAsync<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return (T)await resourceProvider.GetSettingAsync((LambdaExpression)expression, instanceName);
        }

        [NotNull]
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
            var uri = settingMetadata.CreateUri(instanceName);
            var settingInfo =
                await
                    resourceProvider
                        .GetAsync(uri, PopulateProviderInfo(settingMetadata));

            if (settingInfo.Exists)
            {
                using (var memoryStream = new MemoryStream())
                using (var streamReader = new StreamReader(memoryStream))
                {
                    await settingInfo.CopyToAsync(memoryStream);
                    memoryStream.TryRewind();
                    var json = await streamReader.ReadToEndAsync();
                    var converter = GetOrAddDeserializer(settingMetadata.MemberType);
                    return converter.Convert(json, settingMetadata.MemberType);
                }
            }
            else
            {
                throw DynamicException.Create("SettingNotFound", $"Could not find '{uri}'.");
            }
        }

        #endregion

        #region SetValue overloads

        [ItemNotNull]
        public static async Task<IResourceProvider> SetSettingAsync<T>([NotNull] this IResourceProvider resourceProvider, [NotNull] Expression<Func<T>> expression, [CanBeNull] T newValue, [CanBeNull] string instanceName = null)
        {
            if (resourceProvider == null) throw new ArgumentNullException(nameof(resourceProvider));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingMetadata = SettingMetadata.FromExpression(expression, false);
            var uri = settingMetadata.CreateUri(instanceName);

            //var settingInfo =
            //    await
            //        resourceProvider
            //            .GetAsync(uri, PopulateProviderInfo(settingMetadata));

            //if (settingInfo.Exists)
            {
                //settingMetadata
                //    .Validations
                //    .Validate(settingName, newValue);

                newValue.Validate(settingMetadata.Validations, uri);
                var resource = ResourceHelper.CreateStream(newValue);

                // todo - fix put-async
                await resourceProvider.PutAsync(uri, resource.Stream, PopulateProviderInfo(settingMetadata, ResourceMetadata.Empty.Format(resource.Format)));
            }

            return resourceProvider;
        }

        [NotNull]
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
            var uri = settingMetadata.CreateUri(instanceName);
            var value = await resourceProvider.GetSettingAsync(expression, instanceName);
            settingMetadata.SetValue(value.Validate(settingMetadata.Validations, uri));

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
                var uri = settingMetadata.CreateUri(instanceName);
                settingMetadata.SetValue(value.Validate(settingMetadata.Validations, uri));
            }

            return resourceProvider;
        }

        #endregion

        private static object Validate(this object value, IEnumerable<ValidationAttribute> validations, UriString uri)
        {
            foreach (var validation in validations)
            {
                validation.Validate(value, uri);
            }

            return value;
        }

        private static ResourceMetadata PopulateProviderInfo(SettingMetadata settingMetadata, ResourceMetadata metadata = null)
        {
            return
                (metadata ?? ResourceMetadata.Empty)
                .Add(ResourceMetadataKeys.ProviderCustomName, settingMetadata.ProviderName)
                .Add(ResourceMetadataKeys.ProviderDefaultName, settingMetadata.ProviderType?.ToPrettyString());
        }

        #region Helpers

        // todo - put everything in here into a resource-serializer

        private static ITypeConverter _converter = TypeConverter.Empty;

        private static JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters =
            {
                new StringEnumConverter(),
                new Reusable.Utilities.JsonNet.ColorConverter()
            }
        };

        private static IImmutableSet<Type> _stringTypes = new[]
            {
                typeof(string),
                typeof(Enum),
                typeof(TimeSpan),
                typeof(DateTime),
                typeof(Color)
            }
            .ToImmutableHashSet();

        private static ITypeConverter GetOrAddDeserializer(Type toType)
        {
            if (_converter.CanConvert(typeof(string), toType))
            {
                return _converter;
            }

            var converter = CreateJsonConverter(typeof(JsonToObjectConverter<>), toType);
            return (_converter = _converter.Add(converter));
        }

        private static ITypeConverter GetOrAddSerializer(Type fromType)
        {
            if (_converter.CanConvert(fromType, typeof(string)))
            {
                return _converter;
            }

            var converter = CreateJsonConverter(typeof(ObjectToJsonConverter<>), fromType);
            return (_converter = _converter.Add(converter));
        }

        private static ITypeConverter CreateJsonConverter(Type converterType, Type valueType)
        {
            var converterGenericType = converterType.MakeGenericType(valueType);
            return (ITypeConverter)Activator.CreateInstance(converterGenericType, _settings, _stringTypes);
        }

        #endregion
    }
}