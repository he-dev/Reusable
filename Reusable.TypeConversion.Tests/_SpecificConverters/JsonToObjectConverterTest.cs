﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class JsonToObjectConverterTest
    {
        [TestMethod]
        public void Convert_JsonTypeName_Interface()
        {
            var json = @"{ ""$type"": ""Reusable.Converters.Tests.Foo2, Reusable.Converters.Tests"" }";

            var converter = TypeConverter.Empty
                .Add<JsonToObjectConverter<Foo>>();

            var foo = converter.Convert(json, typeof(Foo));
            foo.Verify().IsInstanceOfType(typeof(Foo2));
        }

        [TestMethod]
        public void Convert_JsonTypeName_AbstractClass()
        {
            var json = @"{ ""$type"": ""Reusable.Converters.Tests.Bar1, Reusable.Converters.Tests"" }";

            var converter = TypeConverter.Empty
                .Add<JsonToObjectConverter<Bar>>();

            var bar = converter.Convert(json, typeof(Bar));
            bar.Verify().IsInstanceOfType(typeof(Bar1));
        }

        [TestMethod]
        public void Convert_JsonArray_ArrayInt32()
        {
            var json = @"[1, 2, 3]";

            var converter = TypeConverter.Empty
                .Add<JsonToObjectConverter<int[]>>()
                .Add<StringToInt32Converter>();

            var result = converter.Convert(json, typeof(int[])) as int[];
            result.Verify().IsNotNull().SequenceEqual(new int[] { 1, 2, 3 });
        }
    }

    internal interface Foo { }

    internal class Foo1 : Foo { }

    internal class Foo2 : Foo { }

    internal abstract class Bar { }

    internal class Bar1 : Bar { }

    internal class Bar2 : Bar { }
}