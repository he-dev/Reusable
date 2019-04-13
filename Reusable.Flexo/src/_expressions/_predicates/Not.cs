﻿using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias("!")]
    public class Not : PredicateExpression, IExtension<bool>
    {
        public Not(ILogger<Not> logger) : base(logger, nameof(Not)) { }

        public IExpression Value { get; set; }

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            if (context.TryPopExtensionInput(out bool input))
            {
                return (Name, !input, context);
            }
            else
            {
                return (Name, !Value.Invoke(context).Value<bool>(), context);
            }
        }
    }
}