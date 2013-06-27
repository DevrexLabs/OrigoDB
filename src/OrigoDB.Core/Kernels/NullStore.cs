using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{
    internal class NullStore : IStore
    {
        public void VerifyCanLoad()
        {

        }

        public Model LoadMostRecentSnapshot(out long lastEntryId)
        {
            lastEntryId = -1;
            return null;
        }

        public void WriteSnapshot(Model model, long lastEntryId)
        {
            throw new NotImplementedException();
        }

        public void VerifyCanCreate()
        {
        }

        public void Create(Model model)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            
        }

        public bool Exists
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<JournalEntry> GetJournalEntries()
        {
            return Enumerable.Empty<JournalEntry>();
        }

        public IEnumerable<JournalEntry> GetJournalEntriesFrom(long entryId)
        {
            return Enumerable.Empty<JournalEntry>();
        }

        public IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime)
        {
            return Enumerable.Empty<JournalEntry>();
        }

        public IJournalWriter CreateJournalWriter(long lastEntryId)
        {
            throw new NotImplementedException();
        }
    }
}
