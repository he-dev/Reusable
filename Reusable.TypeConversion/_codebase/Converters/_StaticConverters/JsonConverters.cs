﻿using System;
using Newtonsoft.Json;

namespace Reusable.Converters
{
    public class JsonToObjectConverter<T> : StaticConverter<String, T>
    {
        public override T Convert(string value, ConversionContext context)
        {
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings
            {
                Culture = context.Culture,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }

    public class ObjectToJsonConverter<T> : StaticConverter<T, String>
    {
        public override string Convert(T value, ConversionContext context)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                Culture = context.Culture,
                Formatting = Formatting.Indented
            });
        }
    }
}
