﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.Reflection;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig.Data
{
    using static SettingNameParser;
    using Token = SettingNameToken;

    [PublicAPI]
    public class SettingName
    {
        private readonly IDictionary<SettingNameToken, ReadOnlyMemory<char>> _tokens;

        public static readonly string Format = "[Prefix:][Name.space+][Type.]Member[,Instance]";

        public SettingName(
            [CanBeNull] string prefix,
            [CanBeNull] string schema,
            [CanBeNull] string type,
            [NotNull] string member,
            [CanBeNull] string instance
        )
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            _tokens = new Dictionary<SettingNameToken, ReadOnlyMemory<char>>
            {
                [Token.Prefix] = prefix is null ? ReadOnlyMemory<char>.Empty : new ReadOnlyMemory<char>(prefix.ToCharArray()),
                [Token.Namespace] = schema is null ? ReadOnlyMemory<char>.Empty : new ReadOnlyMemory<char>(schema.ToCharArray()),
                [Token.Type] = type is null ? ReadOnlyMemory<char>.Empty : new ReadOnlyMemory<char>(type.ToCharArray()),
                [Token.Member] = new ReadOnlyMemory<char>(member.ToCharArray()),
                [Token.Instance] = instance is null ? ReadOnlyMemory<char>.Empty : new ReadOnlyMemory<char>(instance.ToCharArray()),
            };
        }

        public SettingName([NotNull] IDictionary<SettingNameToken, ReadOnlyMemory<char>> tokens)
        {
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        }

        public SettingName(UriString uri)
        {
            if (uri.IsRelative) throw new ArgumentException();
            if (uri.Scheme != "setting") throw new ArgumentException();

            var names = uri.Path.Value.Split('.');

            _tokens = new Dictionary<SettingNameToken, ReadOnlyMemory<char>>
            {
                [Token.Prefix] = uri.Query.TryGetValue("prefix", out var prefix) ? new ReadOnlyMemory<char>(prefix.Value.ToCharArray()) : ReadOnlyMemory<char>.Empty,
                [Token.Namespace] = names.Length == 3 ? new ReadOnlyMemory<char>(names[names.Length - 3].ToCharArray()) : ReadOnlyMemory<char>.Empty,
                [Token.Type] = names.Length >= 2 ? new ReadOnlyMemory<char>(names[names.Length - 2].ToCharArray()) : ReadOnlyMemory<char>.Empty,
                [Token.Member] = names.Length >= 1 ? new ReadOnlyMemory<char>(names[names.Length - 1].ToCharArray()) : ReadOnlyMemory<char>.Empty,
                [Token.Instance] = uri.Query.TryGetValue("instance", out var instance) ? new ReadOnlyMemory<char>(instance.Value.ToCharArray()) : ReadOnlyMemory<char>.Empty,
            };
        }

        public ReadOnlyMemory<char> this[Token token] => _tokens.TryGetValue(token, out var t) ? t : default;

        [CanBeNull]
        [AutoEqualityProperty]
        public string Prefix => this[Token.Prefix].ToString();

        [CanBeNull]
        [AutoEqualityProperty]
        public string Namespace => this[Token.Namespace].ToString();

        [CanBeNull]
        [AutoEqualityProperty]
        public string Type => this[Token.Type].ToString();

        [NotNull]
        [AutoEqualityProperty]
        public string Member => this[Token.Member].ToString();

        [CanBeNull]
        [AutoEqualityProperty]
        public string Instance => this[Token.Instance].ToString();

        [NotNull]
        public static SettingName Parse([NotNull] string text) => new SettingName(Tokenize(text));

        public static SettingName FromMetadata(SettingMetadata settingMetadata, string instance)
        {
            return new SettingName
            (
                prefix: settingMetadata.Prefix,
                schema: settingMetadata.Namespace,
                type: settingMetadata.TypeName,
                member: settingMetadata.MemberName,
                instance: instance
            );
        }

        public override string ToString()
        {
            return
                new StringBuilder()
                    .Append(this[Token.Prefix].IsEmpty ? default : $"{Prefix}{Separator.Prefix}")
                    .Append(this[Token.Namespace].IsEmpty ? default : $"{Namespace}{Separator.Namespace}")
                    .Append(this[Token.Type].IsEmpty ? default : $"{Type}{Separator.Type}")
                    .Append(this[Token.Member])
                    .Append(this[Token.Instance].IsEmpty ? default : $"{Separator.Member}{Instance}")
                    .ToString();
        }

        //public static implicit operator SettingName(string settingName) => Parse(settingName);

        public static implicit operator string(SettingName settingName) => settingName?.ToString();

        public static implicit operator SoftString(SettingName settingName) => settingName?.ToString();

        public static implicit operator UriString(SettingName settingName)
        {
            var path = new[]
            {
                settingName.Namespace?.Replace('.', '-'),
                settingName.Type,
                settingName.Member,
            };

            var query = (ImplicitString)new (ImplicitString Key, ImplicitString Value)[]
            {
                ("prefix", settingName.Prefix),
                ("instance", settingName.Instance)
            }
            .Where(x => x.Value)
            .Select(x => $"{x.Key}={x.Value}")
            .Join("&");

            return $"setting:{path.Where(Conditional.IsNotNullOrEmpty).Join(".")}{(query ? $"?{query}" : string.Empty)}";
        }

        public static bool operator ==(SettingName x, SettingName y) => AutoEquality<SettingName>.Comparer.Equals(x, y);

        public static bool operator !=(SettingName x, SettingName y) => !(x == y);

        #region IEquatable<SettingName>

        public bool Equals(SettingName other) => AutoEquality<SettingName>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is SettingName settingName && Equals(settingName);

        public override int GetHashCode() => AutoEquality<SettingName>.Comparer.GetHashCode(this);

        #endregion
    }


}