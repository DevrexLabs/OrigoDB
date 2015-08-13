using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace OrigoDB.Core.Storage.Sql
{
    public class SqlCommandStore : CommandStore
    {
        private SqlProvider _provider;

        public SqlCommandStore(EngineConfiguration config)
            :base(config)
        {
            
        }

        public override void Initialize()
        {
            base.Initialize();
            var connectionString = _config.Location.RelativeLocation;

            string tableName;

            var settings = ConfigurationManager.ConnectionStrings[connectionString];
            if (settings != null) Inititialize(settings);
            else if (connectionString.Contains("=")) Initialize(connectionString);
            else throw new Exception("Unrecognized sql connection identifier. expected connection string name or connection string");
        }

        private void Initialize(string connectionString)
        {
            var config = ConfigDictionary.FromDelimitedString(connectionString);
            var settings = new ConnectionStringSettings();
            settings.ProviderName = config.Get("Provider", () => MsSqlProvider.ProviderName);
            var tableName = GetTableName(config);
            settings.ConnectionString = connectionString;
            _provider = SqlProvider.Create(settings, tableName);
            _provider.Initialize();

        }

        private void Inititialize(ConnectionStringSettings settings)
        {
            var config = ConfigDictionary.FromDelimitedString(settings.ConnectionString);
            var tableName = GetTableName(config);
            _provider = SqlProvider.Create(settings, tableName);
            _provider.Initialize();
        }

        private string GetTableName(ConfigDictionary config)
        {
            return config.Get("Table", () => "origo_journal");
        }

        protected override IEnumerable<JournalEntry> GetJournalEntriesFromImpl(ulong entryId)
        {
            return _provider.ReadJournalEntries(entryId, b => _formatter.FromByteArray<object>(b));
        }

        /// <summary>
        /// Not implemented. SqlCommandStore is not stream based.
        /// </summary>
        public override Stream CreateJournalWriterStream(ulong firstEntryId = 1)
        {
            throw new NotImplementedException("SqlCommandStore is not stream based");
        }

        /// <summary>
        /// Creates a SqlJournalWriter
        /// </summary>
        /// <returns></returns>
        protected override IJournalWriter CreateStoreSpecificJournalWriter()
        {
            return new SqlJournalWriter(_formatter, _provider);
        }

    }
}
