using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : PredicateExpression, IExtension<IEnumerable<IExpression>>
    {
        public Contains() : base(nameof(Contains), ExpressionContext.Empty) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Value { get; set; }

        public IExpression Comparer { get; set; }

        protected override ExpressionResult<bool> InvokeCore(IExpressionContext context)
        {
            var value = Value.Invoke(context).Value<object>();
            var comparer = Comparer?.Invoke(context).ValueOrDefault<IEqualityComparer<object>>() ?? EqualityComparer<object>.Default;
            var values = ExtensionInputOrDefault(ref context, Values).Values<object>();
            return (values.Any(x => comparer.Equals(value, x)), context);
        }
    }        
}