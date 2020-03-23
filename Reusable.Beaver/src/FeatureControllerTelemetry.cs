using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;

namespace Reusable.Beaver
{
    /// <summary>
    /// This class adds telemetry logging to feature-controller.
    /// </summary>
    public class FeatureControllerTelemetry : IFeatureController
    {
        private readonly IFeatureController _controller;

        private readonly ILogger<FeatureControllerTelemetry> _logger;

        public FeatureControllerTelemetry(IFeatureController controller, ILogger<FeatureControllerTelemetry> logger)
        {
            _controller = controller;
            _logger = logger;
        }

        public Feature this[string name] => _controller[name];

        public bool TryGet(string name, out Feature feature) => _controller.TryGet(name, out feature);

        public void Add(Feature feature) => _controller.Add(feature);

        public bool TryRemove(string name, out Feature feature) => _controller.TryRemove(name, out feature);

        public async Task<FeatureResult<T>> Use<T>(string name, Func<Task<T>> onEnabled, Func<Task<T>>? onDisabled = default, object? parameter = default)
        {
            var feature = this[name];

            using var featureScope = _logger.BeginScope().WithCorrelationHandle("UseFeature");
            _logger.Log(Execution.Context.WorkItem("feature", new { name = feature.Name, tags = feature.Tags, policy = feature.Policy.GetType().ToPrettyString() }));

            return await _controller.Use(name, onEnabled, onDisabled, parameter).ContinueWith(t =>
            {
                _logger.Scope().Flow().Push(t.Exception);

                // if (this.IsEnabled(Feature.Telemetry.CreateName(name)))
                // {
                //     _logger.Log(Execution.Context.Service().Meta("featureTelemetry", new
                //     {
                //         name = feature.Name,
                //         tags = feature.Tags,
                //         policy = feature.Policy.GetType().ToPrettyString()
                //     }).Exception(t.Exception));
                // }

                return t.Result;
            });
        }

        public IEnumerator<Feature> GetEnumerator() => _controller.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_controller).GetEnumerator();
    }
}