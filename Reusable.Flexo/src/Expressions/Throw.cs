using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Throw : Expression<IExpression>
    {
        public Throw() : base(LoggerDummy.Instance, nameof(Throw)) { }

        //public string Exception { get; set; }

        public IExpression Message { get; set; }

        protected override Constant<IExpression> InvokeCore()
        {
            throw DynamicException.Create(Name.ToString(), Message.Invoke().Value<string>());
        }
    }
}