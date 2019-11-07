using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Throw : Expression<IExpression>
    {
        public Throw() : base(EmptyLogger.Instance, nameof(Throw)) { }

        //public string Exception { get; set; }

        public IExpression Message { get; set; }

        protected override IExpression ComputeValue(IImmutableContainer context)
        {
            throw DynamicException.Create(Name.ToString(), Message.Invoke(context).Value<string>());
        }
    }
}