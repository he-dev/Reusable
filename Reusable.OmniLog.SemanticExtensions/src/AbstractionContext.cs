﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OmniLog.SemanticExtensions.Attachements;
using Reusable.Reflection;

namespace Reusable.OmniLog.SemanticExtensions
{
    public interface IAbstractionContext
    {
        IImmutableDictionary<string, object> Values { get; }

        /// <summary>
        /// Logs this context. This method supports the Framework infrastructure and is not intended to be used directly in your code.
        /// </summary>
        void Log(ILogger logger, Action<Log> configureLog);
    }

    public readonly struct AbstractionContext : IAbstractionLayer, IAbstractionCategory, IAbstractionContext
    {
        public static class PropertyNames
        {
            public const string Layer = nameof(Layer);
            public const string Category = nameof(Category);
            public const string Identifier = nameof(Identifier);
            public const string Snapshot = nameof(Snapshot);
        }

        public static IDictionary<string, LogLevel> LayerLogLevel = new Dictionary<string, LogLevel>
        {
            [nameof(AbstractionExtensions.Business)] = LogLevel.Information,
            [nameof(AbstractionExtensions.Infrastructure)] = LogLevel.Debug,
            [nameof(AbstractionExtensions.Presentation)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.IO)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.Database)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.Network)] = LogLevel.Trace,
        };

        public static IDictionary<string, LogLevel> CategoryLogLevel = new Dictionary<string, LogLevel>
        {
            [nameof(AbstractionLayerExtensions.Counter)] = LogLevel.Information,
            [nameof(AbstractionLayerExtensions.Meta)] = LogLevel.Debug,
            [nameof(AbstractionLayerExtensions.Variable)] = LogLevel.Trace,
            [nameof(AbstractionLayerExtensions.Property)] = LogLevel.Trace,
            [nameof(AbstractionLayerExtensions.Argument)] = LogLevel.Trace,
        };

        public AbstractionContext(IImmutableDictionary<string, object> values, string property, [CallerMemberName] string name = null)
        {
            Values = values.Add(property, name);
        }

        public AbstractionContext(string property, [CallerMemberName] string name = null)
            : this(ImmutableDictionary<string, object>.Empty, property, name)
        {
        }

        public AbstractionContext(IImmutableDictionary<string, object> values)
        {
            Values = values;
        }

        public IImmutableDictionary<string, object> Values { get; }

        public void Log(ILogger logger, Action<Log> configureLog)
        {
            // In some cases Snapshot can already have an Identifier. If this is then case then use this one instead of enumerating its properties.
            var properties =
                Values.TryGetValue(PropertyNames.Snapshot, out var snapshot)
                    ? Values.TryGetValue(PropertyNames.Identifier, out var identifier)
                        ? new[] { new KeyValuePair<string, object>((string)identifier, snapshot) }
                        : snapshot.EnumerateProperties()
                    : Enumerable.Empty<KeyValuePair<string, object>>();

            var mappedLogLevel =
                CategoryLogLevel.TryGetValue((string)Values[PropertyNames.Category], out var logLevel)
                    ? logLevel
                    : LayerLogLevel.TryGetValue((string)Values[PropertyNames.Layer], out logLevel)
                        ? logLevel
                        : throw DynamicException.Create("LogLevelNotFound", $"Neither category '{Values[PropertyNames.Category]}' nor layer '{Values[PropertyNames.Layer]}' map to a valid log-level.");

            var values = Values;

            foreach (var dump in properties)
            {
                logger.Log(mappedLogLevel, log =>
                {
                    // todo - this could be a loop over 'values'
                    log.With(PropertyNames.Identifier, dump.Key);
                    log.With(PropertyNames.Category, values[PropertyNames.Category]);
                    log.With(PropertyNames.Layer, values[PropertyNames.Layer]);
                    log.With(Snapshot.BagKey, dump.Value);
                    configureLog?.Invoke(log);
                });
            }
        }
    }
}
