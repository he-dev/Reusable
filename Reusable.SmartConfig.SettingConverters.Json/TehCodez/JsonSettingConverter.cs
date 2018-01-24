﻿using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Converters;
using Reusable.ThirdParty.JsonNetUtilities;

namespace Reusable.SmartConfig.SettingConverters
{

    /*
     *
     * This converter is build on top of the Reusable.Converters.
     * It initializes each json-converter on demand.
     * 
     */

    [PublicAPI]
    public class JsonSettingConverter : SettingConverter
    {
        [NotNull]
        private ITypeConverter _converter;

        [NotNull]
        private JsonSerializerSettings _settings;

        [NotNull]
        private ISet<Type> _stringTypes;

        public JsonSettingConverter() : base(new[] { typeof(string) })
        {
            _converter = TypeConverter.Empty;
            _settings = new JsonSerializerSettings();
            _stringTypes = new HashSet<Type>();
        }

        [NotNull]
        public static readonly JsonSettingConverter Default = new JsonSettingConverter
        {
            Settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters =
                {
                    new StringEnumConverter(),
                    new JsonColorConverter()
                }
            },
            StringTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(Enum),
                typeof(TimeSpan),
                typeof(DateTime),
                typeof(Color)
            },
        };

        [NotNull]
        public JsonSerializerSettings Settings
        {
            get => _settings;
            set => _settings = value ?? throw new ArgumentNullException(nameof(Settings));
        }

        [NotNull, ItemNotNull]
        public ISet<Type> StringTypes
        {
            get => _stringTypes;
            set => _stringTypes = value ?? throw new ArgumentNullException(nameof(StringTypes));
        }

        protected override object DeserializeCore(object value, Type toType)
        {
            if (!(value is string)) throw new ArgumentException($"Unsupported type '{toType.Name}'. Only {typeof(string).Name} is allowed.");

            AddJsonToObjectConverter(toType);
            return _converter.Convert(value, toType);
        }

        protected override object SerializeCore(object value, Type targetType)
        {
            var fromType = value.GetType();
            AddObjectToJsonConverter(fromType);
            return (string)_converter.Convert(value, targetType);
        }

        private void AddJsonToObjectConverter(Type toType)
        {
            if (_converter.CanConvert(typeof(string), toType))
            {
                return;
            }

            var jsonToObjectConverter = CreateJsonToObjectConverter(toType);
            _converter = _converter.Add(jsonToObjectConverter);
        }

        private ITypeConverter CreateJsonToObjectConverter(Type toType)
        {
            var jsonToObjectConverterType = typeof(JsonToObjectConverter<>).MakeGenericType(toType);
            return (ITypeConverter)Activator.CreateInstance(jsonToObjectConverterType, _settings, StringTypes);
        }

        private void AddObjectToJsonConverter(Type fromType)
        {
            if (_converter.CanConvert(fromType, typeof(string)))
            {
                return;
            }

            var objectToJsonConverter = CreateObjectToJsonConverter(fromType);
            _converter = _converter.Add(objectToJsonConverter);
        }

        private ITypeConverter CreateObjectToJsonConverter(Type fromType)
        {
            var objectToJsonConverterType = typeof(ObjectToJsonConverter<>).MakeGenericType(fromType);
            return (ITypeConverter)Activator.CreateInstance(objectToJsonConverterType, _settings, StringTypes);
        }
    }
}