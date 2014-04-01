using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using OrigoDB.Core.Journaling;
using OrigoDB.Core.Logging;

namespace OrigoDB.Core.Storage
{
    [Flags]
    public enum StoreState
    {
        Uninitialized = 1, 
        Initialized   = 2,
        Loaded        = 4
    };

    public abstract class Store : IStore
    {

        protected static ILogger _log = LogProvider.Factory.GetLoggerForCallingType();
        protected StoreState _storeState = StoreState.Uninitialized;
        protected EngineConfiguration _config;
        protected ISerializer _serializer;
        protected List<Snapshot> _snapshots;
        protected JournalAppender _journalAppender;


        protected abstract IJournalWriter CreateStoreSpecificJournalWriter(ulong lastEntryId);
        protected abstract Snapshot WriteSnapshotImpl(Model model, ulong lastAppliedEntryId);
        public abstract IEnumerable<JournalEntry> GetJournalEntriesFrom(ulong entryId);
        public abstract IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime);
        public abstract Model LoadSnapshot(Snapshot snapshot);
        protected abstract IEnumerable<Snapshot> ReadSnapshotMetaData();
        public abstract Stream CreateJournalWriterStream(ulong firstEntryId = 1);

        public JournalAppender GetAppender()
        {
            ExpectState(StoreState.Loaded);
            return _journalAppender;
        }

        private Model MostRecentSnapshot(ref ulong lastEntryId)
        {
            Model result = null;
            var latestSnapshot = Snapshots.LastOrDefault();
            if (latestSnapshot != null)
            {
                lastEntryId = latestSnapshot.LastEntryId;
                result = LoadSnapshot(latestSnapshot);
                result.SnapshotRestored();
            }
            return result;

        }

        public Model LoadModel(Type modelType = null)
        {
            ExpectState(StoreState.Initialized);

            ulong currentEntryId = 0;
            Model result = MostRecentSnapshot(ref currentEntryId);


            if (result == null)
            {
                var firstJournalEntry = GetJournalEntries().Take(1).OfType<JournalEntry<ModelCreated>>().SingleOrDefault();
                if (firstJournalEntry != null) modelType = firstJournalEntry.Item.Type;
                if (modelType == null) throw new InvalidOperationException("No model type present");
                result = (Model)Activator.CreateInstance(modelType);
            }


            //Restore model
            foreach (var commandEntry in this.CommandEntriesFrom(currentEntryId+1))
            {
                commandEntry.Item.Redo(ref result);
                currentEntryId = commandEntry.Id;
            }
            result.JournalRestored();
            _journalAppender = new JournalAppender(currentEntryId + 1, CreateJournalWriter(currentEntryId + 1));

            _storeState = StoreState.Loaded;
            return result;
        }

        protected void ExpectState(StoreState states)
        {
            if ((_storeState & states) == 0) 
            {
                var message = String.Format("Expected state(s) {0}, was {1}", states, _storeState);
                throw new InvalidOperationException(message);
            }
        }

        public ulong LastEntryId
        {
            get
            {
                ExpectState(StoreState.Loaded);
                return _journalAppender.LastEntryId;
            }
        }

        public virtual IEnumerable<Snapshot> Snapshots
        {
            get 
            {
                ExpectState(StoreState.Initialized | StoreState.Loaded);
                return _snapshots;
            }
        }

        protected Store(EngineConfiguration config)
        {
            _config = config;
        }



        public virtual IEnumerable<JournalEntry> GetJournalEntries()
        {
            return GetJournalEntriesFrom(1);
        }

        public void WriteSnapshot(Model model)
        {
            ExpectState(StoreState.Loaded);
            Snapshot snapshot = WriteSnapshotImpl(model, LastEntryId);
            _snapshots.Add(snapshot);
        }

        public virtual IJournalWriter CreateJournalWriter(ulong lastEntryId)
        {
            IJournalWriter writer = CreateStoreSpecificJournalWriter(lastEntryId);
            return _config.AsyncronousJournaling
                       ? new AsynchronousJournalWriter(writer)
                       : writer;
        }

        public virtual void Init()
        {
            ExpectState(StoreState.Uninitialized);
            _snapshots = new List<Snapshot>();
            foreach (var snapshot in ReadSnapshotMetaData())
            {
                _snapshots.Add(snapshot);
            }

            _serializer = _config.CreateSerializer();
            _storeState = StoreState.Initialized;
        }

        public bool IsEmpty
        {
            get
            {
                if (_storeState == StoreState.Loaded) return false;
                ExpectState(StoreState.Initialized);
                return !Snapshots.Any() && !GetJournalEntries().Any();
            }
        }

        protected void AssertEmpty()
        {
            if (!IsEmpty) throw new InvalidOperationException("Store must be empty");
        }

        public virtual void Create<T>() where T : Model, new()
        {
            AssertEmpty();
            Create(typeof(T));
        }

        public virtual void Create(Type modelType)
        {
            AssertEmpty();
            var writer = CreateJournalWriter(0);
            var appender = new JournalAppender(1, writer);
            appender.AppendModelCreated(modelType);
            writer.Close();
        }

        public virtual void Create(Model model)
        {
            AssertEmpty();
            WriteSnapshotImpl(model, 0);
        }

    }
}
