﻿using System;

namespace Reusable.Converters
{
    public class StringToDecimalConverter : StaticConverter<String, Decimal>
    {
        public override Decimal Convert(string value, ConversionContext context)
        {
            return Decimal.Parse(value, context.Culture);
        }
    }

    public class DecimalToStringConverter : StaticConverter<decimal, string>
    {
        public override string Convert(Decimal value, ConversionContext context)
        {
            return value.ToString(context.Culture);
        }
    }
}
