﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Invokes each expression and flows the context from one to the other.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public class Select : CollectionExtension<IEnumerable<IExpression>>
    {
        public Select() : base(default) { }

        public IEnumerable<IExpression>? Values { get => Arg; set => Arg = value; }

        public IExpression? Selector { get; set; }

        protected override IEnumerable<IExpression> ComputeValue(IImmutableContainer context)
        {
            var query =
                from item in GetArg(context)
                let selector = Selector ?? item
                select selector.Invoke(context.BeginScopeWithArg(item));

            return query.ToList();
        }
    }
}