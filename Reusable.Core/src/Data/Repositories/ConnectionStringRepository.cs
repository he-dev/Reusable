﻿using System;
using System.Configuration;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Data.Repositories
{
    public interface IConnectionStringRepository
    {
        [NotNull, ContractAnnotation("nameOrConnectionString: null => halt")]
        string GetConnectionString([NotNull] string nameOrConnectionString);
    }

    public class ConnectionStringRepository : IConnectionStringRepository
    {
        [NotNull]
        public static readonly IConnectionStringRepository Default = new ConnectionStringRepository();

        public string GetConnectionString(string nameOrConnectionString)
        {
            if (string.IsNullOrEmpty(nameOrConnectionString)) { throw new ArgumentNullException(nameof(nameOrConnectionString)); }

            var match = Regex.Match(nameOrConnectionString, @"\Aname=(?<name>.+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return 
                    ConfigurationManager
                        .ConnectionStrings[match.Groups["name"].Value]
                        ?.ConnectionString 
                        ?? throw DynamicException.Factory.CreateDynamicException(
                            $"ConnectionStringNotFound{nameof(Exception)}",
                            $"Connection string {match.Groups["name"].Value.QuoteWith("'")} not found.", 
                            null
                        );
            }
            else
            {
                return nameOrConnectionString;
            }
        }
    }
}
