﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;

namespace Reusable.IOnymous
{
    public class ConnectionStringProvider : SettingProvider
    {
        private readonly ITypeConverter _uriStringToSettingIdentifierConverter;

        public ConnectionStringProvider(ITypeConverter uriStringToSettingIdentifierConverter = null)
            : base(Metadata.Empty)
        {
            _uriStringToSettingIdentifierConverter = uriStringToSettingIdentifierConverter;
        }
        
        public ITypeConverter Converter { get; set; } = new NullConverter();

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, Metadata metadata)
        {
            var settingIdentifier = (string)_uriStringToSettingIdentifierConverter?.Convert(uri, typeof(string)) ?? uri;
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = FindConnectionStringSettings(exeConfig, settingIdentifier);
            return Task.FromResult<IResourceInfo>(new ConnectionStringInfo(uri, settings?.ConnectionString));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream stream, Metadata metadata)
        {
            using (var valueReader = new StreamReader(stream))
            {
                var value = await valueReader.ReadToEndAsync();

                var settingIdentifier = (string)_uriStringToSettingIdentifierConverter?.Convert(uri, typeof(string)) ?? uri;
                var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = FindConnectionStringSettings(exeConfig, settingIdentifier);

                if (settings is null)
                {
                    exeConfig.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(settingIdentifier, value));
                }
                else
                {
                    settings.ConnectionString = value;
                }

                exeConfig.Save(ConfigurationSaveMode.Minimal);

                return await GetAsync(uri);
            }
        }

        [CanBeNull]
        private static ConnectionStringSettings FindConnectionStringSettings(Configuration exeConfig, string key)
        {
            return
                exeConfig
                    .ConnectionStrings
                    .ConnectionStrings
                    .Cast<ConnectionStringSettings>()
                    .SingleOrDefault(x => SoftString.Comparer.Equals(x.Name, key));
        }
    }

    internal class ConnectionStringInfo : ResourceInfo
    {
        [CanBeNull] private readonly string _value;

        internal ConnectionStringInfo([NotNull] UriString uri, [CanBeNull] string value) 
            : base(uri, Metadata.Empty.Format(MimeType.Text))
        {
            _value = value;
        }

        public override bool Exists => !(_value is null);

        public override long? Length => _value?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            // ReSharper disable once AssignNullToNotNullAttribute - this isn't null here
            using (var valueStream = _value.ToStreamReader())
            {
                await valueStream.BaseStream.CopyToAsync(stream);
            }
        }
    }
}