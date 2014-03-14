using System;
using System.Collections.Generic;
using OrigoDB.Core.Journaling;
using OrigoDB.Core.Logging;

namespace OrigoDB.Core
{

    /// <summary>
    /// CommandJournal is responsible for reading and writing journal entries to the journal
    /// </summary>
	public sealed class CommandJournal //: ICommandJournal
	{
        enum JournalState
        {
            Closed,
            Open
        }

        private IJournalWriter _writer;
		readonly IStore _storage;
        private JournalState _state;
	    private static ILogger _log = LogProvider.Factory.GetLoggerForCallingType();
        private ulong _lastEntryId;

        /// <summary>
        /// Id of the last entry written to the journal
        /// </summary>
        public ulong LastEntryId
        {
            get { return _lastEntryId; }
        }


        public CommandJournal(IStore storage)
        {
            _storage = storage;
        }


        public IEnumerable<JournalEntry<Command>> GetEntriesFrom(ulong entryId)
        {
            return CommittedCommandEntries(() => _storage.GetJournalEntriesFrom(entryId));
        }

        public IEnumerable<JournalEntry<Command>> GetEntriesFrom(DateTime pointInTime)
        {
            return CommittedCommandEntries(() => _storage.GetJournalEntriesBeforeOrAt(pointInTime));
        }

        internal IEnumerable<JournalEntry<Command>> CommittedCommandEntries(Func<IEnumerable<JournalEntry>> enumerator)
        {
            lock (this)
            {
                JournalState preState = _state;
                TransitionTo(JournalState.Closed);

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

                TransitionTo(preState);
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
            TransitionTo(JournalState.Open);
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


        private void TransitionTo(JournalState newState)
        {
            if (_state != newState)
            {
                if (newState == JournalState.Open) Open();
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
