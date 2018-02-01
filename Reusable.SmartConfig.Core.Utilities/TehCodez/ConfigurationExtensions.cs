using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Utilities.Reflection;

namespace Reusable.SmartConfig.Utilities
{
    public static class ConfigurationExtensions
    {
        //public static (IConfiguration Configuration, TObject Object) From<TObject>([NotNull] this IConfiguration config)
        //{
        //    return (config, default(TObject));
        //}

        //[CanBeNull]
        //public static TValue GetValue<TObject, TValue>(this (IConfiguration Configuration, TObject obj) t, [NotNull] Expression<Func<TObject, TValue>> expression)
        //{
        //    return t.Configuration.GetValue<TValue>(expression);
        //}

        #region GetValue overloads

        [CanBeNull]
        private static T GetValue<T>([NotNull] this IConfiguration config, [NotNull] LambdaExpression expression, [CanBeNull] string instance = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingContext = new SettingContext(expression, instance);

            var settingValue = config.GetValue(settingContext.SettingName, typeof(T), settingContext.DataStoreName) ?? settingContext.DefaultValue;

            settingContext
                .Validations
                .Validate(settingContext.SettingName, settingValue);

            return (T)settingValue;
        }

        [CanBeNull]
        public static T GetValue<T>([NotNull] this IConfiguration config, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instance = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return config.GetValue<T>((LambdaExpression)expression, instance);
        }

        [CanBeNull]
        public static T GetValue<T>([NotNull] this IConfiguration config, [NotNull] SoftString settingName)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            return (T)config.GetValue(settingName, typeof(T), null);
        }

        [NotNull]
        public static Lazy<T> GetValueLazy<T>([NotNull] this IConfiguration config, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instance = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return Lazy.Create(() => config.GetValue(expression, instance));
        }

        #endregion

        #region SetValue overloads

        [NotNull]
        public static IConfiguration SetValue<T>([NotNull] this IConfiguration config, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instance = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingContext = new SettingContext(expression, instance);

            var settingValue = config.GetValue(settingContext.SettingName, typeof(T), settingContext.DataStoreName) ?? settingContext.DefaultValue;

            settingContext
                .Validations
                .Validate(settingContext.SettingName, settingValue);
            
            config.SetValue(settingContext.SettingName, settingValue, null);
            return config;
        }

        #endregion

        #region AssignValue overloads

        /// <summary>
        /// Assigns the same setting value to the specified member.
        /// </summary>
        [NotNull]
        public static IConfiguration AssignValue<T>([NotNull] this IConfiguration configuration, [NotNull] Expression<Func<T>> expression, [CanBeNull] string instance = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var settingContext = new SettingContext(expression, instance);

            var value = configuration.GetValue(expression, instance);
            settingContext.SetValue(value);
            
            return configuration;
        }

        /// <summary>
        /// Assigns setting values to all members decorated with the the SmartSettingAttribute.
        /// </summary>
        [NotNull]
        public static IConfiguration AssignValues<T>([NotNull] this IConfiguration configuration, [NotNull] T obj, [CanBeNull] string instance = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var properties = 
                typeof(T)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute<SmartSettingAttribute>() != null);

            foreach (var property in properties)
            {
                // Create a lambda-expression so that we can reuse the extensions for it we already have.
                var expression = Expression.Lambda(
                    Expression.Property(
                        Expression.Constant(obj),
                        property.Name
                    )
                );

                var settingContext = new SettingContext(expression, instance);
                var value = configuration.GetValue(settingContext.SettingName, property.PropertyType, settingContext.DataStoreName);
                settingContext.SetValue(value);
            }
            return configuration;
        }

        #endregion

        private static void Validate(this IEnumerable<ValidationAttribute> validations, SettingName settingName, object value)
        {
            foreach (var validation in validations)
            {
                validation.Validate(value, $"Setting {settingName.ToString().QuoteWith("'")} is not valid.");
            }
        }
    }
}