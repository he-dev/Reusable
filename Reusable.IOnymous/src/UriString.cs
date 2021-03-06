﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class UriString : IEquatable<UriString>, IEquatable<string>
    {
        // https://regex101.com/r/sd288W/1
        // using 'new[]' for _nicer_ syntax
        private static readonly string UriPattern = string.Join(string.Empty, new[]
        {
            /* language=regexp */ @"^(?:(?<scheme>\w+):)?",
            /* language=regexp */ @"(?:\/\/(?<authority>[a-z0-9\.\-_:]+))?",
            /* language=regexp */ @"(?:\/?(?<path>[a-z0-9\/:\.\-\%_@]+))",
            /* language=regexp */ @"(?:\?(?<query>[a-z0-9=&]+))?",
            /* language=regexp */ @"(?:#(?<fragment>[a-z0-9]+))?"
        });

        public static readonly IEqualityComparer<UriString> Comparer = EqualityComparerFactory<UriString>.Create
        (
            equals: (x, y) =>
            {
                var ignoreScheme = x.IsIOnymous() || y.IsIOnymous();
                var xUri = ignoreScheme ? x.ToString(string.Empty) : x.ToString();
                var yUri = ignoreScheme ? y.ToString(string.Empty) : y.ToString();
                return SoftString.Comparer.Equals(xUri, yUri);
            },
            getHashCode: (obj) =>
            {
                var ignoreScheme = obj.IsIOnymous();
                return SoftString.Comparer.GetHashCode(ignoreScheme ? obj.ToString(string.Empty) : obj.ToString());
            });

        public UriString([NotNull] string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            uri = UriStringHelper.Normalize(uri);

            var uriMatch = Regex.Match
            (
                uri,
                UriPattern,
                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
            );

            if (!uriMatch.Success)
            {
                throw new ArgumentException(paramName: nameof(uri), message: $"'{uri}' is not a valid Uri.");
            }

            Scheme = uriMatch.Groups["scheme"].Value;
            Authority = uriMatch.Groups["authority"].Value;
            Path = new UriStringComponent(UriStringHelper.Encode(uriMatch.Groups["path"].Value.Trim('/')));
            Query =
                uriMatch.Groups["query"].Success
                    ? Regex
                        .Matches
                        (
                            uriMatch.Groups["query"].Value,
                            @"(?:^|&)(?<key>[a-z0-9]+)(?:=(?<value>[a-z0-9]+))?",
                            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
                        )
                        .Cast<Match>()
                        .ToImmutableDictionary
                        (
                            m => (SoftString)m.Groups["key"].Value,
                            m => (SoftString)m.Groups["value"].Value
                        )
                    : ImmutableDictionary<SoftString, SoftString>.Empty;
            Fragment = uriMatch.Groups["fragment"].Value;
        }

        public UriString(string scheme, string path)
            : this($"{scheme}:{UriStringHelper.Normalize(path)}")
        {
        }

        public UriString([NotNull] UriString first, [NotNull] UriString second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            Scheme = first.Scheme;
            Authority = first.Authority;
            Path = 
                first.Path.Original 
                    ? first.Path.Original.ToString().Trim('/') + "/" + second.Path.Original.ToString().Trim('/') 
                    : second.Path.Original.ToString().Trim('/');
            Query = second.Query;
            Fragment = second.Fragment;
        }

        public UriString([NotNull] ImmutableUpdate update)
        {
            update.Bind(this);
        }

        public SoftString Scheme { get; }

        public SoftString Authority { get; }

        public UriStringComponent Path { get; }

        public IImmutableDictionary<SoftString, SoftString> Query { get; }

        public SoftString Fragment { get; }

        public bool IsAbsolute => Scheme;

        public bool IsRelative => !IsAbsolute;

        public override string ToString() => ToString(Scheme.ToString());

        public string ToString(SoftString scheme) => string.Join(string.Empty, GetComponents(scheme));

        private IEnumerable<SoftString> GetComponents(SoftString scheme)
        {
            if (scheme)
            {
                yield return $"{scheme.ToString()}:";
            }

            if (Authority)
            {
                yield return $"//{Authority.ToString()}{(Path.Original ? "/" : string.Empty)}";
            }

            yield return Path.Original;

            if (Query.Any())
            {
                var queryPairs =
                    Query
                        .OrderBy(x => x.Key)
                        .Select(x => $"{x.Key.ToString()}{(x.Value ? "=" : string.Empty)}{x.Value.ToString()}");
                yield return $"?{string.Join("&", queryPairs)}";
            }

            if (Fragment)
            {
                yield return $"#{Fragment.ToString()}";
            }
        }

        #region IEquatable

        public bool Equals(UriString other) => Comparer.Equals(this, other);

        public bool Equals(string other) => Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is UriString uri && Equals(uri);

        public override int GetHashCode() => Comparer.GetHashCode(this);

        #endregion

        #region Helpers

        #endregion

        #region operators

        public static implicit operator UriString(string uri) => new UriString(uri);

        //public static implicit operator UriString((string scheme, string path) uri) => new UriString(uri.scheme, uri.path);

        public static implicit operator string(UriString uri) => uri.ToString();

        public static UriString operator +(UriString left, UriString right) => new UriString(left, right);

        public static bool operator ==(UriString left, UriString right) => Comparer.Equals(left, right);

        public static bool operator !=(UriString left, UriString right) => !(left == right);

        #endregion
    }

    public class UriStringComponent
    {
        public UriStringComponent([NotNull] SoftString value) => Original = value ?? throw new ArgumentNullException(nameof(value));

        [NotNull]
        public SoftString Original { get; }

        [NotNull]
        public SoftString Decoded => UriStringHelper.Decode(Original.ToString());

        public static implicit operator UriStringComponent(string value) => new UriStringComponent(value);

        public static implicit operator string(UriStringComponent component) => component.Original.ToString();

        public static implicit operator UriString(UriStringComponent component) => new UriString(component.Original.ToString());
    }
}