using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;

namespace Reusable.Data
{
    // Protects the user form using an unsupported interface by mistake.
    public interface INamespace { }

    [PublicAPI]
    public static class From<T> where T : INamespace
    {
        public static Selector<TMember> Select<TMember>([NotNull] Expression<Func<T, TMember>> selector)
        {
            return new Selector<TMember>(selector);
        }
    }

    [PublicAPI]
    public class Selector
    {
        private readonly LambdaExpression _selector;

        public Selector(LambdaExpression selector)
        {
            _selector = selector;
            Keys = this.GetKeys().ToImmutableList();
        }

        protected Selector(LambdaExpression selector, IImmutableList<Key> keys)
        {
            _selector = selector;
            Keys = keys;
        }

        public Type DeclaringType => _selector.ToMemberExpression().Member.DeclaringType;

        public PropertyInfo Property => (PropertyInfo)_selector.ToMemberExpression().Member;

        [NotNull, ItemNotNull]
        public IImmutableList<Key> Keys { get; }

        public static Selector FromProperty(Type declaringType, PropertyInfo property)
        {
            // () => x.Member
            var selector =
                Expression.Lambda(
                    Expression.Property(
                        Expression.Constant(null, declaringType),
                        property.Name
                    )
                );
            
            return new Selector(selector);
        }

        public override string ToString() => SelectorFormatters.Plain(this);

        public virtual Selector Index(string index)
        {
            if (Keys.OfType<IndexKey>().Any())
            {
                throw new InvalidOperationException("This selector already contains an index.");
            }

            return new Selector(_selector, Keys.Add(new IndexKey(index)));
        }

        [NotNull]
        public static implicit operator string(Selector selector) => selector.ToString();

        [NotNull]
        public static implicit operator SoftString(Selector selector) => selector.ToString();
    }

    public class Selector<T> : Selector
    {
        public Selector(LambdaExpression selector) : base(selector) { }

        //private Selector(Selector<T> other, IImmutableList<Key> keys) : base(other, keys) { }
        
//        public override Selector<T> Index(string index)
//        {
//            if (Keys.OfType<IndexKey>().Any()) throw new InvalidOperationException("This selector already contains an index.");
//
//            return new Selector<T>(this, Keys.Add(new IndexKey(index)));
//        }
    }
    
    public static class SelectorFormatters
    {
        public static readonly Func<Selector, string> Plain = selector => selector.Keys.Join(string.Empty);
    }

    public static class SelectorExtensions
    {
        [NotNull, ItemNotNull]
        internal static IEnumerable<Key> GetKeys(this Selector selector)
        {
            var keyEnumerators =
                from m in selector.Property.AncestorTypesAndSelf()
                where m.IsDefined(typeof(KeyEnumeratorAttribute))
                select m.GetCustomAttribute<KeyEnumeratorAttribute>();

            return (keyEnumerators.FirstOrDefault() ?? new KeyEnumeratorAttribute()).EnumerateKeys(selector);
        }
        
        public static IImmutableList<Selector> AddFrom<T>(this IImmutableList<Selector> selectors)
        {
            var newSelectors =
                from p in typeof(T).GetProperties()
                select Selector.FromProperty(typeof(T), p);

            return selectors.AddRange(newSelectors);
        }
        
        public static IEnumerable<Selector> Where<T>(this IEnumerable<Selector> featureSelectors, params string[] names) where T : Attribute, IEnumerable<string>
        {
            if (!names.Any()) throw new ArgumentException("You need to specify at least one tag.");

            return
                from f in featureSelectors
                where f.Names<T>().IsSubsetOf(names, SoftString.Comparer)
                select f;
        }
        
        private static IEnumerable<string> Names<T>(this Selector selector) where T : Attribute, IEnumerable<string>
        {
            var names =
                from m in selector.Property.AncestorTypesAndSelf()
                from ts in m.GetCustomAttributes<T>()
                from t in ts
                select t;

            return names.Distinct(SoftString.Comparer);
        }
    }
}