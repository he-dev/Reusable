using System;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class SettingNameAttribute : Attribute
    {
        private readonly string _name;
        public SettingNameAttribute(string name) => _name = name.NullIfEmpty() ?? throw new ArgumentNullException(paramName: nameof(name), message: "Custom setting name must not be empty.");
        public override string ToString() => _name;
    }
}