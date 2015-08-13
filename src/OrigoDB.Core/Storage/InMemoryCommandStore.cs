using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core.Storage;
using System.IO;

namespace OrigoDB.Core.Test
{
    /// <summary>
    /// The InMemoryCommandStore is a non durable CommandStore implementation
    /// intended for integration/system tests.
    /// </summary>
    /// <remarks>
    /// It's implemented using a list of memory streams to mimic 
    /// the behavior of FileCommandStore as closely as possible.
    /// </remarks>
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

        //State mementos by Location.OfJournal.
        //this pattern enables persistence across consecutive instances based on the location
        static readonly Dictionary<string, InMemoryCommandStoreState> _states
            = new Dictionary<string, InMemoryCommandStoreState>();

        readonly InMemoryCommandStoreState _state;


        public InMemoryCommandStore(EngineConfiguration config)
            : base(config)
        {
            if (_config.HasAlternativeSnapshotPath()) throw new NotSupportedException("SnapshotPath must be same as JournalPath");

            string key = _config.JournalPath;
            if (!_states.ContainsKey(key)) _states.Add(key, new InMemoryCommandStoreState());
            _state = _states[key];
        }

        protected override IEnumerable<JournalEntry> GetJournalEntriesFromImpl(ulong entryId)
        {
            return _state.Journal.SelectMany(
                journalSegment => _formatter
                    .ReadToEnd<JournalEntry>(new MemoryStream(journalSegment.ToArray()))
                    .SkipWhile(e => e.Id < entryId));
        }

        /// <summary>
        /// Sets up and returns a MemoryStream for writing journal entries
        /// </summary>
        /// <param name="startId"></param>
        /// <returns></returns>
        public override Stream CreateJournalWriterStream(ulong startId = 1)
        {
            var stream = new MemoryStream();
            _state.Journal.Add(stream);
            return stream;
        }
    }
}
