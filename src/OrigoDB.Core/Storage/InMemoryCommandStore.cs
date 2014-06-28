using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core.Storage;
using System.IO;

namespace OrigoDB.Core.Test
{
    /// <summary>
    /// The InMemoryCommandStore is a non durable CommandStore implementation
    /// intended for tests
    /// </summary>
    public class InMemoryCommandStore : CommandStore
    {
        /// <summary>
        /// Memento pattern
        /// </summary>
        private class InMemoryCommandStoreState
        {
            public readonly List<MemoryStream> Journal;

            public InMemoryCommandStoreState()
            {
                Journal = new List<MemoryStream>();
            }
        }

        //State mementos by Location.OfJournal
        static readonly Dictionary<string, InMemoryCommandStoreState> _states
            = new Dictionary<string, InMemoryCommandStoreState>();

        readonly InMemoryCommandStoreState _state;


        public InMemoryCommandStore(EngineConfiguration config)
            : base(config)
        {
            if (_config.Location.HasAlternativeSnapshotLocation) throw new NotSupportedException();

            string key = _config.Location.OfJournal;
            if (!_states.ContainsKey(key)) _states.Add(key, new InMemoryCommandStoreState());
            _state = _states[key];
        }



        protected override IJournalWriter CreateStoreSpecificJournalWriter()
        {
            return new StreamJournalWriter(_config, CreateJournalWriterStream);
        }


        public override IEnumerable<JournalEntry> GetJournalEntriesFrom(ulong entryId)
        {
            return _state.Journal.SelectMany(
                journalSegment => _formatter
                    .ReadToEnd<JournalEntry>(new MemoryStream(journalSegment.ToArray()))
                    .SkipWhile(e => e.Id < entryId));
        }

        public override IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime)
        {
            return GetJournalEntriesFrom(0).TakeWhile(entry => entry.Created <= pointInTime);
        }

        public override Stream CreateJournalWriterStream(ulong startId = 1)
        {
            var stream = new MemoryStream();
            _state.Journal.Add(stream);
            return stream;
        }
    }
}
