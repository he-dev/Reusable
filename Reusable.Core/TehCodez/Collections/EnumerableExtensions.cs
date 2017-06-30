﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Collections
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Applies a specified function to the corresponding elements of two sequences, producing a sequence of the results.
        /// Unlike the default Zip method this one doesn't stop if enumerating one of the collections is complete.
        /// It continues enumerating the longer collection and return default(T) for the other one.
        /// </summary>
        /// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
        /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
        /// <param name="first">The first input sequence.</param>
        /// <param name="second">The second input sequence.</param>
        /// <param name="resultSelector">A function that specifies how to combine the corresponding elements of the two sequences.</param>
        /// <returns>An IEnumerable<T> that contains elements of the two input sequences, combined by resultSelector.</returns>
        public static IEnumerable<TResult> ZipWithDefault<TFirst, TSecond, TResult>(
            this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null) { throw new ArgumentNullException(nameof(first)); }
            if (second == null) { throw new ArgumentNullException(nameof(second)); }
            if (resultSelector == null) { throw new ArgumentNullException(nameof(resultSelector)); }

            using (var enumeratorFirst = first.GetEnumerator())
            using (var enumeratorSecond = second.GetEnumerator())
            {
                var isEndOfFirst = !enumeratorFirst.MoveNext();
                var isEndOfSecond = !enumeratorSecond.MoveNext();
                while (!isEndOfFirst || !isEndOfSecond)
                {
                    yield return resultSelector(
                        isEndOfFirst ? default(TFirst) : enumeratorFirst.Current,
                        isEndOfSecond ? default(TSecond) : enumeratorSecond.Current);

                    isEndOfFirst = !enumeratorFirst.MoveNext();
                    isEndOfSecond = !enumeratorSecond.MoveNext();
                }
            }
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T item)
        {
            if (enumerable == null) { throw new ArgumentNullException(nameof(enumerable)); }

            return enumerable.Concat(new[] { item });
        }

        public static string Join<T>(this IEnumerable<T> values, string separator) => string.Join(
            separator ?? throw new ArgumentNullException(nameof(separator)),
            values ?? throw new ArgumentNullException(nameof(values)));

        public static IEnumerable<TArg> Except<TArg, TProjection>(this IEnumerable<TArg> first, IEnumerable<TArg> second, Func<TArg, TProjection> projection)
        {
            if (first == null) { throw new ArgumentNullException(nameof(first)); }
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (projection == null) throw new ArgumentNullException(nameof(projection));
            if (ReferenceEquals(first, projection)) { throw new ArgumentException(paramName: nameof(projection), message: "Projection must be an anonymous type."); }

            return first.Except(second, new ProjectionComparer<TArg, TProjection>(projection));
        }

        public static IEnumerable<string> Quote<T>(this IEnumerable<T> values)
        {
            return values.Select(x => $"\"{x}\"");
        }

        public static IEnumerable<T> TakeOrRepeat<T>(this IEnumerable<T> values, int count)
        {
            if (values == null) { throw new ArgumentNullException(nameof(values)); }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(
                   paramName: nameof(count),
                   actualValue: count,
                   message: "Count must be greater or equal 0."
                );
            }

            var counter = 0;
            var enumerator = values.GetEnumerator();
            var moveNext = new Func<bool>(() =>
            {
                if (enumerator.MoveNext())
                {
                    return true;
                }
                // Could not move-next. Reset enumerator and try again.
                enumerator = values.GetEnumerator();
                return enumerator.MoveNext();
            });

            while (counter++ < count)
            {
                if (moveNext())
                {
                    yield return enumerator.Current;
                }
                else
                {
                    yield break;
                }
            }
        }

        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue value)
        {
            if (@this.ContainsKey(key))
            {
                return false;
            }
            else
            {
                @this.Add(key, value);
                return true;
            }
        }

        public static bool StartsWith<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<TSource>.Default;
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            using (var e1 = first.GetEnumerator())
            using (var e2 = second.GetEnumerator())
            {
                while (e2.MoveNext())
                {
                    if (!(e1.MoveNext() && comparer.Equals(e1.Current, e2.Current))) return false;
                }
                if (e2.MoveNext()) return false;
            }
            return true;
        }
    }
}
