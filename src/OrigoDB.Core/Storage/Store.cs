using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OrigoDB.Core.Logging;

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
            get {
                return _snapshots;
            }
        }

        public ulong LastEntryId { get; protected set; }


        protected Store(EngineConfiguration config)
        {
            _config = config;
            _serializer = _config.CreateSerializer();
            _journal = new CommandJournal(this);
        }

        protected abstract IJournalWriter CreateStoreSpecificJournalWriter(ulong lastEntryId);
        protected abstract Snapshot WriteSnapshotImpl(Model model);
        public abstract IEnumerable<JournalEntry> GetJournalEntriesFrom(ulong entryId);
        public abstract IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime);
        public abstract Model LoadMostRecentSnapshot(out ulong lastEntryId);
        public abstract void VerifyCanLoad();
        public abstract void VerifyCanCreate();
        public abstract void Create(Model model);
        protected abstract IEnumerable<Snapshot> LoadSnapshots();
        public abstract Stream CreateJournalWriterStream(ulong firstEntryId = 1);

        public virtual IEnumerable<JournalEntry> GetJournalEntries()
        {
            return GetJournalEntriesFrom(1);
        }

        public void WriteSnapshot(Model model)
        {
            if(Snapshots.Any(ss => ss.LastEntryId == LastEntryId))
            {
                _log.Debug("Snapshot already exists");
                return;
            }
            Snapshot snapshot = WriteSnapshotImpl(model);
            _snapshots.Add(snapshot);
        }

        public virtual IJournalWriter CreateJournalWriter(ulong lastEntryId)
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

        private CommandJournal _journal;

        public Model LoadModel()
        {
            ulong lastEntryIdExecuted;
            Model model = LoadMostRecentSnapshot(out lastEntryIdExecuted);

            model.SnapshotRestored();
            var commandJournal = new CommandJournal(this);
            foreach (var command in commandJournal.GetEntriesFrom(lastEntryIdExecuted).Select(entry => entry.Item))
            {
                command.Redo(ref model);
            }
            model.JournalRestored();
            LastEntryId = lastEntryIdExecuted;
            return model;
        }


        public void AppendCommand(Command command)
        {
            _journal.Append(command);
        }

        public void InvalidatePreviousCommand()
        {
            _journal.WriteRollbackMarker();
        }
    }
}
