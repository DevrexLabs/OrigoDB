using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Common.Logging;

namespace OrigoDB.Core.Storage
{
    public abstract class Store : IStore
    {
        protected EngineConfiguration _config;
        protected ISerializer _serializer;
        protected static ILogger _log = LogProvider.Factory.GetLoggerForCallingType();

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


        protected Store(EngineConfiguration config)
        {
            _config = config;
            _serializer = _config.CreateSerializer();
        }

        protected abstract IJournalWriter CreateStoreSpecificJournalWriter(long lastEntryId);
        protected abstract Snapshot WriteSnapshotImpl(Model model, long lastEntryId);
        public abstract IEnumerable<JournalEntry> GetJournalEntriesFrom(long entryId);
        public abstract IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime);
        public abstract Model LoadMostRecentSnapshot(out long lastEntryId);
        public abstract void VerifyCanLoad();
        public abstract void VerifyCanCreate();
        public abstract void Create(Model model);
        protected abstract IEnumerable<Snapshot> LoadSnapshots();
        public abstract Stream CreateJournalWriterStream(long firstEntryId = 1);

        public virtual IEnumerable<JournalEntry> GetJournalEntries()
        {
            return GetJournalEntriesFrom(1);
        }

        public void WriteSnapshot(Model model, long lastEntryId)
        {
            if(Snapshots.Any(ss => ss.LastEntryId == lastEntryId))
            {
                _log.Debug("Snapshot already exists");
                return;
            }
            Snapshot snapshot = WriteSnapshotImpl(model, lastEntryId);
            _snapshots.Add(snapshot);
        }

        public virtual IJournalWriter CreateJournalWriter(long lastEntryId)
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
    }
}
