using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Beaver
{
    public class FeatureTelemetry : IFeatureAgent
    {
        private readonly IFeatureAgent _agent;

        private readonly ILogger<FeatureTelemetry> _logger;

        public FeatureTelemetry(IFeatureAgent agent, ILogger<FeatureTelemetry> logger)
        {
            _agent = agent;
            _logger = logger;
        }

        public Feature this[string name] => _agent[name];
        
        public void AddOrUpdate(Feature feature) => _agent.AddOrUpdate(feature);

        public bool TryRemove(string name, out Feature feature) => _agent.TryRemove(name, out feature);

        public async Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> ifEnabled, Func<Task<T>>? ifDisabled = default, object? parameter = default)
        {
            using (_logger.BeginScope().WithCorrelationHandle("UseFeature").UseStopwatch())
            {
                return await _agent.Use(name, ifEnabled, ifDisabled, parameter).ContinueWith(t =>
                {
                    if (this.IsEnabled(Feature.Telemetry.CreateName(name)))
                    {
                        var feature = this[name];
                        _logger.Log(Abstraction.Layer.Service().Meta(new
                        {
                            feature = new
                            {
                                name = feature.Name,
                                tags = feature.Tags,
                                policy = feature.Policy.GetType().ToPrettyString()
                            }
                        }), log => log.Exception(t.Exception));
                    }

                    return t.Result;
                });
            }
        }

        public IEnumerator<Feature> GetEnumerator() => _agent.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_agent).GetEnumerator();
    }
}