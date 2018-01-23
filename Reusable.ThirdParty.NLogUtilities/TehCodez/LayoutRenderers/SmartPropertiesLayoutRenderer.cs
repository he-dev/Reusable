﻿using System;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace Reusable.ThirdParty.NLogUtilities.LayoutRenderers
{
    [UsedImplicitly, PublicAPI]
    [LayoutRenderer(Name)]
    public class SmartPropertiesLayoutRenderer : LayoutRenderer
    {
        public const string Name = "smart-properties";

        public string Key { get; set; }

        public bool IgnoreCase { get; set; }

        public string Culture { get; set; } = string.Empty;

        public string Format { get; set; }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(Key)) throw new InvalidOperationException("You need to specify the property key.");

            var comparer = IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            var property = logEvent.Properties.SingleOrDefault(p => p.Key is string s && comparer.Equals(Key, s));
            if (!(property.Value is null))
            {
                builder.AppendFormat(new CultureInfo(Culture), $"{{0{(Format is null ? string.Empty : ":" + Format)}}}", property.Value);
            }
        }

        public static void Register()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition(Name, typeof(SmartPropertiesLayoutRenderer));
        }
    }
}