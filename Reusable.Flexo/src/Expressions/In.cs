using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class In : ScalarExtension<bool>, IFilter
    {
        public In() : base(default)
        {
            Matcher = Constant.DefaultComparer;
        }

        public IExpression Value
        {
            get => Arg;
            set => Arg = value;
        }

        public IEnumerable<IExpression> Values { get; set; }

        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression Matcher { get; set; }
        
        protected override bool ComputeValue(IImmutableContainer context)
        {
            var value = GetArg(context).Invoke(context).Value;
            var comparer = this.GetEqualityComparer(context);
            return Values.Enabled().Any(x => comparer.Equals(value, x.Invoke(context).Value));
        }

    }
}