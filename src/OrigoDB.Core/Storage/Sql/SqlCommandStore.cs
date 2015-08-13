using System;
using System.Collections.Generic;
using System.IO;

namespace OrigoDB.Core.Storage.Sql
{

    /// <summary>
    /// Command store implementation using a backing Sql database.
    /// </summary>
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
            _provider = SqlProvider.Create(_config.SqlSettings);
            _provider.Initialize();
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
