using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reusable.Collections;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Datastores
{
    public class Memory : Datastore, IEnumerable<ISetting>
    {
        public Memory() : base(new[] { typeof(string) }) { }

        protected Memory(IEnumerable<Type> supportedTypes) : base(supportedTypes) { }

        protected override ISetting ReadCore(IEnumerable<CaseInsensitiveString> names)
        {
            var setting =
                (from name in names
                 from x in Data
                 where x.Name.Equals(name)
                 select x).FirstOrDefault();
            return setting;
        }

        protected override void WriteCore(ISetting setting)
        {
            //var obsoleteSettings =
            //    (from x in Data
            //     where x.Name.StartsWith(settings.Key)
            //     select x).ToList();
            //obsoleteSettings.ForEach(x => Data.Remove(x));

            //foreach (var setting in settings) Add(setting);

            //return obsoleteSettings.Count;
        }

        public List<ISetting> Data { [DebuggerStepThrough] get; [DebuggerStepThrough] set; } = new List<ISetting>();

        #region IEnumerable

        public void Add(ISetting setting) => Data.Add(setting);

        public void Add(string name, object value) => Data.Add(new Setting
        {
            Name = name,
            Value = value
        });

        public Memory AddRange(IEnumerable<ISetting> settings)
        {
            foreach (var setting in settings) Add(setting);
            return this;
        }

        public IEnumerator<ISetting> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}