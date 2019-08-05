﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Abstractions.Data
{
    public class Log : IEnumerable<(ItemKey<SoftString> Key, object Value)>
    {
        public static readonly string DefaultItemTag = "Property";

        private readonly IDictionary<ItemKey<SoftString>, object> _data;

        public Log()
        {
            _data = new Dictionary<ItemKey<SoftString>, object>();
        }

        private Log(IDictionary<ItemKey<SoftString>, object> data)
        {
            _data = new Dictionary<ItemKey<SoftString>, object>(data);
        }

        public static Log Empty() => new Log();

        public object this[string name]
        {
            get => this[(name, DefaultItemTag)];
            set => this[(name, DefaultItemTag)] = value;
        }

        public object this[(string name, string tag) key]
        {
            get => _data[key];
            set => _data[key] = value;
        }

        public Log Clone() => new Log(_data);

        public Log SetItem((string Name, string Tag) key, object value)
        {
            _data[(key.Name, key.Tag ?? DefaultItemTag)] = value;
            return this;
        }

        public T GetItemOrDefault<T>((string Name, string Tag) key, T defaultValue = default)
        {
            return _data.TryGetValue((key.Name, key.Tag ?? DefaultItemTag), out var obj) && obj is T value ? value : defaultValue;
        }

        public bool TryGetItem<T>((string Name, string Tag) key, out T value)
        {
            if (_data.TryGetValue((key.Name, key.Tag ?? DefaultItemTag), out var obj))
            {
                switch (obj)
                {
                    case T t:
                        value = t;
                        return true;

                    case object o:
                        value = (T)o;
                        return true;

                    default:
                        value = default;
                        return false;
                }
            }
            else
            {
                value = default;
                return false;
            }
        }

        public bool RemoveItem((string Name, string Tag) key) => _data.Remove((key.Name, key.Tag ?? DefaultItemTag));

        public IEnumerator<(ItemKey<SoftString> Key, object Value)> GetEnumerator()
        {
            return _data.Select(x => (x.Key, x.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static class PropertyNames
        {
            public static readonly string Logger = nameof(Logger);
            public static readonly string Category = nameof(Category);
            public static readonly string Level = nameof(Level);
            public static readonly string Message = nameof(Message);
            public static readonly string Exception = nameof(Exception);
            public static readonly string Elapsed = nameof(Elapsed);
            public static readonly string Timestamp = nameof(Timestamp);
            public static readonly string CallerMemberName = nameof(CallerMemberName);
            public static readonly string CallerLineNumber = nameof(CallerLineNumber);
            public static readonly string CallerFilePath = nameof(CallerFilePath);
            public static readonly string Metadata = nameof(Metadata);

            //public static readonly SoftString Scope = nameof(Scope);
            //public static readonly SoftString CorrelationId = nameof(CorrelationId);
            //public static readonly SoftString Context = nameof(Context);

            // This field can be used to remove a property from log.
            public static readonly object Unset = new object();
        }
    }

    public static class LogExtensions
    {
        public static Log SetProperty(this Log log, string name, object value)
        {
            return log.SetItem((name, "Property"), value);
        }

        // public static Log SetMetadata(this Log log, string name, object value)
        // {
        //     return log.SetItem((name, LogItemTag.Metadata), value);
        // }

        public static bool TryGetProperty<T>(this Log log, string name, out T value)
        {
            return log.TryGetItem((name, "Property"), out value);
        }

        // public static bool TryGetMetadata<T>(this Log log, string name, out T value)
        // {
        //     return log.TryGetItem((name, LogItemTag.Metadata), out value);
        // }
    }

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public readonly struct ItemKey<T> : IEquatable<ItemKey<T>>
    {
        public ItemKey(SoftString name, T tag)
        {
            Name = name;
            Tag = tag;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValue(x => x.Name.ToString());
            builder.DisplayValue(x => x.Tag.ToString());
        });

        [AutoEqualityProperty]
        public SoftString Name { get; }

        [AutoEqualityProperty]
        public T Tag { get; }

        public override int GetHashCode() => AutoEquality<ItemKey<T>>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => obj is ItemKey<T> key && Equals(key);

        public bool Equals(ItemKey<T> other) => AutoEquality<ItemKey<T>>.Comparer.Equals(this, other);

        public override string ToString() => $"{Name.ToString()}#{Tag.ToString()}";

        public void Deconstruct(out string name, out T tag)
        {
            name = Name.ToString();
            tag = Tag;
        }

        public static implicit operator ItemKey<T>((string name, T tag) key) => new ItemKey<T>(key.name, key.tag);
    }

    public static class LogItemTags
    {
        /// <summary>
        /// These items are ready to be logged.
        /// </summary>
        public static readonly string Property = nameof(Property);

        /// <summary>
        /// These items need to be processed before they can be a 'Property'
        /// </summary>
        public static readonly string Metadata = nameof(Metadata);

        /// <summary>
        /// These items need to be processed by LoggerSerialize before they can be a 'Property'
        /// </summary>
        public static readonly string Serializable = nameof(Serializable);
    }
}