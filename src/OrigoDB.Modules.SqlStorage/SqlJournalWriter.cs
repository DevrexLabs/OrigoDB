using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrigoDB.Core;

namespace OrigoDB.Modules.SqlStorage
{
    public class SqlJournalWriter : IJournalWriter
    {
        private SqlStore _store;

        public SqlJournalWriter(SqlStore store)
        {
            _store = store;
        }

        public void Write(JournalEntry item)
        {
            _store.WriteEntry(item);
        }

        public void Close()
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}
