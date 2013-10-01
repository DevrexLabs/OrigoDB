using System;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core
{
    public class NullStore : IStore
    {

        Model _model;

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
            _model = model;
        }

        public void Load()
        {
            
        }

        public bool Exists
        {
            get { return _model != null; }
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


        public System.IO.Stream CreateJournalWriterStream(long firstEntryId = 1)
        {
            throw new NotImplementedException();
        }


        public Model LoadModel()
        {
            return _model;
        }
    }
}
