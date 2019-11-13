using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog.Nodes
{
    // Reroutes items from one property to the other: Meta#Dump --> Snapshot#Dump 
    public class RenameNode : LoggerNode
    {
        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();

        protected override void invoke(LogEntry request)
        {
            foreach (var (key, value) in Mappings.Select(x => (x.Key, x.Value)))
            {
                if (request.TryGetProperty<Log>(key, out var property))
                {
                    request.Add<Delete>(key, default);
                    request.Add<Log>(value, property.Value);
                }
            }

            invokeNext(request);
        }
    }
}