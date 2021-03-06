﻿using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.SemanticExtensions.Attachments
{
    public class Snapshot : LogAttachment
    {
        public static readonly string BagKey = $"{nameof(Snapshot)}Bag";

        private readonly ISerializer _serializer;

        /// <summary>
        /// Creates a new Snapshot attachment. Uses JsonStateSerializer by default.
        /// </summary>
        public Snapshot(ISerializer serializer = null) : base(nameof(Snapshot))
        {
            _serializer = serializer ?? new JsonSerializer();
        }

        public override object Compute(ILog log)
        {
            return
                log.TryGetValue(BagKey, out var snapshot) 
                    ? _serializer.Serialize(snapshot) 
                    : null;
        }
    }
}