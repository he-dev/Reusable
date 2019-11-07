using Reusable.Data;

namespace Reusable.Flexo
{
    public class Package : Expression<object>
    {
        public Package() : base(default, nameof(Package)) { }

        public IExpression Body { get; set; }

        protected override object ComputeValue(IImmutableContainer context)
        {
            return Body.Invoke(context).Value<object>();
        }
    }
}