using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Storage
{
    public abstract class Store : IStore
    {
        protected EngineConfiguration _config;
        protected ISerializer _serializer;

        private List<Snapshot> _snapshots;
        public IEnumerable<Snapshot> Snapshots
        {
            get
            {
                foreach (var snapshot in _snapshots)
                {
                    yield return snapshot;
                }
            }
        }


        protected abstract IEnumerable<Snapshot> LoadSnapshots();

        protected Store(EngineConfiguration config)
        {
            _config = config;
            _serializer = _config.CreateSerializer();
        }

        protected abstract IJournalWriter CreateStoreSpecificJournalWriter(long lastEntryId);
        protected abstract Snapshot WriteSnapshotImpl(Model model, long lastEntryId);
        public abstract IEnumerable<JournalEntry<Command>> GetJournalEntriesFrom(long sequenceNumber);
        public abstract IEnumerable<JournalEntry<Command>> GetJournalEntriesFrom(DateTime pointInTime);
        public abstract Model LoadMostRecentSnapshot(out long lastSequenceNumber);
        public abstract void VerifyCanLoad();

        public virtual IEnumerable<JournalEntry<Command>> GetJournalEntries()
        {
            return GetJournalEntriesFrom(1);
        }

        public void WriteSnapshot(Model model, long lastEntryId)
        {
            Snapshot snapshot = WriteSnapshotImpl(model, lastEntryId);
            _snapshots.Add(snapshot);
        }

        public IJournalWriter CreateJournalWriter(long lastEntryId)
        {
            IJournalWriter writer = CreateStoreSpecificJournalWriter(lastEntryId);
            return _config.AsyncronousJournaling
                       ? new AsynchronousJournalWriter(writer)
                       : writer;
        }

        public virtual void Load()
        {
            _snapshots = new List<Snapshot>();
            foreach (var snapshot in LoadSnapshots())
            {
                _snapshots.Add(snapshot);
            }
        }

        
        
        public virtual bool Exists
        {
            get
            {
                try
                {
                    VerifyCanLoad();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }



        public abstract void VerifyCanCreate();

        public abstract void Create(Model model);
    }
}
