using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core.Storage;
using System.IO;

namespace OrigoDB.Core.Test
{

    /// <summary>
    /// The InMemoryStore is a non durable Store implementation
    /// intended for test.
    /// </summary>
    public class InMemoryStore : Store
    {
        private class InMemoryStoreState
        {
            public readonly Dictionary<Snapshot, byte[]> Snapshots;
            public readonly List<MemoryStream> Journal;

            public InMemoryStoreState()
            {
                Snapshots = new Dictionary<Snapshot, byte[]>();
                Journal = new List<MemoryStream>();
            }
        }

        static readonly Dictionary<string, InMemoryStoreState> _states
            = new Dictionary<string, InMemoryStoreState>();

        readonly InMemoryStoreState _state;


        public InMemoryStore(EngineConfiguration config)
            : base(config)
        {
            if (_config.Location.HasAlternativeSnapshotLocation) throw new NotSupportedException();

            string key = _config.Location.OfJournal;
            if (!_states.ContainsKey(key)) _states.Add(key, new InMemoryStoreState());
            _state = _states[key];
        }



        protected override IJournalWriter CreateStoreSpecificJournalWriter(ulong lastEntryId)
        {
            return new StreamJournalWriter(this, _config);
        }

        protected override Snapshot WriteSnapshotImpl(Model model, ulong lastEntryId)
        {
            var bytes = _snapshotFormatter.ToByteArray(model);
            var snapshot = new Snapshot(DateTime.Now, lastEntryId);
            _state.Snapshots.Add(snapshot, bytes);
            return snapshot;
        }

        public override IEnumerable<JournalEntry> GetJournalEntriesFrom(ulong entryId)
        {
            return _state.Journal.SelectMany(
                journalSegment => _journalFormatter.ReadToEnd<JournalEntry>(new MemoryStream(journalSegment.ToArray())).SkipWhile(e => e.Id < entryId));
        }

        public override IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime)
        {
            return GetJournalEntriesFrom(0).TakeWhile(entry => entry.Created <= pointInTime);
        }

        public override Model LoadSnapshot(Snapshot snapshot)
        {
            if (! _state.Snapshots.ContainsKey(snapshot))
            {
                throw new ArgumentException("No such snapshot");
            }

            return _snapshotFormatter.FromByteArray<Model>(_state.Snapshots[snapshot]);
        }



        protected override IEnumerable<Snapshot> ReadSnapshotMetaData()
        {
            foreach (var snapshot in _state.Snapshots.Keys.OrderBy(ss => ss.LastEntryId)) yield return snapshot;
        }

        public override Stream CreateJournalWriterStream(ulong firstEntryId = 1)
        {
            var stream = new MemoryStream();
            _state.Journal.Add(stream);
            return stream;
        }
    }
}
