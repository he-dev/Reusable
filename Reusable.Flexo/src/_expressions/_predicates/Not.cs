﻿using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias("!")]
    public class Not : PredicateExpression, IExtension<bool>
    {
        public Not(ILogger<Not> logger) : base(logger, nameof(Not)) { }

        [This]
        public IExpression Value { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context)
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