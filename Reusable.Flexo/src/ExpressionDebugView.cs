using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class ExpressionDebugView
    {
        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayScalar(x => x.Name);
            b.DisplayScalar(x => x.Result);
            b.DisplayScalar(x => x.Description);
        });

        public static ExpressionDebugView Root => new ExpressionDebugView
        {
            Name = "Root",
            Description = $"This is the root node of the {nameof(ExpressionDebugView)}."
        };

        public static RenderTreeNodeValueCallback<ExpressionDebugView, TreeNodePlainView> DefaultRenderTreeNode
        {
            get
            {
                return (debugView, depth) => new TreeNodePlainView
                {
                    Text = $"[{debugView.ExpressionType}] as [{debugView.Name}]: '{FormatResult(debugView.Result)}' ({debugView.Description})",
                    Depth = depth
                };
            }
        }

        public string ExpressionType { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public object Result { get; set; }

        public IList<string> Tags { get; set; }

        private static string FormatResult(object result)
        {
            switch (result)
            {
                case double d: return d.ToString("F2", CultureInfo.InvariantCulture);
                case bool b: return b.ToString();
                case string s: return $"'{s}'";
                case Type t: return $"'{t.ToPrettyString()}'";
                case null: return "null";
                default: return $"'{result.ToString()}'";
            }
        }

        public static class Templates
        {
            public static RenderTreeNodeValueCallback<ExpressionDebugView, TreeNodePlainView> Compact
            {
                get
                {
                    return (debugView, depth) => new TreeNodePlainView
                    {
                        Text = $"{debugView.Name ?? debugView.ExpressionType}: {FormatResult(debugView.Result)}{(debugView.Tags?.Any() == true ? $" {debugView.Tags.Join(" ")}" : string.Empty)}",
                        Depth = depth
                    };
                }
            }
        }
    }
}