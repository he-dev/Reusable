﻿using System;

namespace Reusable.OneTo1.Converters
{
    public class StringToSByteConverter : TypeConverter<String, SByte>
    {
        protected override SByte ConvertCore(IConversionContext<String> context)
        {
            return SByte.Parse(context.Value, context.FormatProvider);
        }
    }

    public class SByteToStringConverter : TypeConverter<sbyte, string>
    {
        protected override string ConvertCore(IConversionContext<SByte> context)
        {
            return context.Value.ToString(context.FormatProvider);
        }
    }
}
