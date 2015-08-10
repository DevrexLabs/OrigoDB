using System;
using System.Collections.Generic;
using System.IO;

namespace OrigoDB.Core.Storage.Sql
{
    public class SqlCommandStore : CommandStore
    {

        public SqlCommandStore(EngineConfiguration config)
            :base(config)
        {
            
        }

        protected override IEnumerable<JournalEntry> GetJournalEntriesFromImpl(ulong entryId)
        {
            throw new NotImplementedException();
        }

        public override Stream CreateJournalWriterStream(ulong firstEntryId = 1)
        {
            throw new NotImplementedException();
        }

        protected override IJournalWriter CreateStoreSpecificJournalWriter()
        {
            //return base.CreateStoreSpecificJournalWriter();
            return new SqlJournalWriter(null);
        }
    }
}
