﻿using Reusable.Shelly.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Shelly.Collections
{
    public class ArgumentCollection
    {
        private readonly Dictionary<string, IList<string>> _arguments = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

        internal ArgumentCollection(IEnumerable<CommandLineArgument> arguments)
        {
            _arguments = arguments.ToDictionary(x => x.Key, x => (IList<string>)x, StringComparer.OrdinalIgnoreCase);
        }

        public StringSet CommandName => _arguments.TryGetValue(string.Empty, out IList<string> values) ? StringSet.CreateCI(values.FirstOrDefault()) : null;

        public IList<string> this[string name]
        {
            get => _arguments.TryGetValue(name, out IList<string> values) ? values : null;
            set => _arguments[name] = value;
        }
    }
}