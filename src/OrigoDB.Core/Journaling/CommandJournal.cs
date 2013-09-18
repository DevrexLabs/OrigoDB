using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OrigoDB.Core.Logging;
using OrigoDB.Core.Journaling;

namespace OrigoDB.Core
{

    /// <summary>
    /// CommandJournal is responsible for reading and writing journal entries to the journal
    /// </summary>
	public class CommandJournal : ICommandJournal
	{
        protected enum JournalState
        {
            Closed,
            Open
        }

        private IJournalWriter _writer;
		protected IStore _storage;
        private JournalState _state;
	    protected static ILogger _log = LogProvider.Factory.GetLoggerForCallingType();
        private long _lastEntryId;

        /// <summary>
        /// Id of the last entry written to the journal
        /// </summary>
        public long LastEntryId
        {
            get { return _lastEntryId; }
        }


        public CommandJournal(IStore storage)
        {
            _storage = storage;
        }


        public IEnumerable<JournalEntry<Command>> GetEntriesFrom(long entryId)
        {

            foreach (var entry in GetCommandEntries(() => _storage.GetJournalEntriesFrom(entryId)))
                yield return entry;
        }

        public IEnumerable<JournalEntry<Command>> GetEntriesFrom(DateTime pointInTime)
        {
            foreach (var entry in GetCommandEntries(() => _storage.GetJournalEntriesBeforeOrAt(pointInTime)))
                yield return entry;
        }

        internal IEnumerable<JournalEntry<Command>> GetCommandEntries(Func<IEnumerable<JournalEntry>> enumerator)
        {
            lock (this)
            {
                JournalState preState = _state;
                SetState(JournalState.Closed);

                JournalEntry<Command> previous = null;

                foreach (var journalEntry in enumerator.Invoke())
                {
                    if (journalEntry is JournalEntry<Command>)
                    {
                        if (previous != null)
                        {
                            _lastEntryId = previous.Id;
                            yield return previous;
                        }
                        previous = journalEntry as JournalEntry<Command>;
                    }
                    else previous = null;
                }
                if (previous != null)
                {
                    _lastEntryId = previous.Id;
                    yield return previous;
                }

                SetState(preState);
            }
        }

		public IEnumerable<JournalEntry<Command>> GetAllEntries()
		{
            return GetEntriesFrom(1);
		}


		private void Open()
		{

            if (_state == JournalState.Open)
            {
                throw new InvalidOperationException("Can't open command journal, already open");
            }

            _writer = _storage.CreateJournalWriter(LastEntryId + 1);
            _state = JournalState.Open;
		}

		public void Append(Command command)
		{
			var entry = new JournalEntry<Command>(++_lastEntryId, command);
		    AppendEntry(entry);
		}

        private void AppendEntry(JournalEntry entry)
        {
            if (_state != JournalState.Open) Open();
            _writer.Write(entry);
        }

        public void Close()
		{
            if (_state == JournalState.Open)
            {
                _writer.Close();
                _writer.Dispose();
                _state = JournalState.Closed;
            }
		}


        private void SetState(JournalState state)
        {
            if (_state != state)
            {
                if (state == JournalState.Open) Open();
                else Close();
            }
        }

        /// <summary>
        /// Write a special entry to the log
        /// </summary>
        public void WriteRollbackMarker()
        {
            AppendEntry(new JournalEntry<RollbackMarker>(_lastEntryId, RollbackMarker.Instance));
        }

        public void Dispose()
        {
            Close();
        }
    }
}
