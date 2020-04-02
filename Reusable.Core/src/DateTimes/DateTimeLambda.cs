﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable
{
    public class DateTimeLambda : IDateTime
    {
        private readonly Func<DateTime> _now;

        public DateTimeLambda(Func<DateTime> now) => _now = now;

        public DateTime Now() => _now();
    }
}