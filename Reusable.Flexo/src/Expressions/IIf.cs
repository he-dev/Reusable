﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    // ReSharper disable once InconsistentNaming - we want this name!
    public class IIf : Extension<object, object>, IFilter
    {
        public IIf() : base(default)
        {
            Matcher = Constant.DefaultComparer;
        }

        public IExpression? Value
        {
            set => Arg = value;
        }

        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression? Matcher { get; set; }

        public IExpression? True { get; set; }

        public IExpression? False { get; set; }

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            if (True is null && False is null) throw new InvalidOperationException($"You need to specify at least one result ({nameof(True)}/{nameof(False)}).");

            var x = GetArg(context);
            var comparer = this.GetEqualityComparer(context);
            return x.SequenceEqual(new object[] { true }, comparer) switch
            {
                true => True?.Invoke(context),
                false => False?.Invoke(context)
            };
        }
    }
}