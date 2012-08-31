using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LiveDomain.Core.Logging;

namespace LiveDomain.Core
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
        protected EngineConfiguration _config;
	    protected static ILog _log = Log.GetLogFactory().GetLogForCallingType();
        private long _lastEntryId;

        /// <summary>
        /// Id of the last entry written to the journal
        /// </summary>
        public long LastEntryId
        {
            get { return _lastEntryId; }
        }


        public CommandJournal(EngineConfiguration config)
        {
            _config = config;
            _storage = config.CreateStorage();
            _storage.Load();
        }


        public IEnumerable<JournalEntry<Command>> GetEntriesFrom(long entryId)
        {
            JournalState preState = _state;
            SetState(JournalState.Closed);

            foreach (var journalEntry in _storage.GetJournalEntriesFrom(entryId))
            {
                _lastEntryId = journalEntry.Id;
                yield return journalEntry;
            }

            SetState(preState);
        }

		public IEnumerable<JournalEntry<Command>> GetAllEntries()
		{
            return GetEntriesFrom(1);
		}


		public void Open()
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
            if (_state != JournalState.Open) Open();
            //{
            //    throw new InvalidOperationException("Can't append to journal when closed");
            //}

			var entry = new JournalEntry<Command>(++_lastEntryId, command);
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


        public IEnumerable<JournalEntry<Command>> GetEntriesFrom(DateTime pointInTime)
        {
            lock (this)
            {
                JournalState preState = _state;
                SetState(JournalState.Closed);

                foreach (var journalEntry in _storage.GetJournalEntriesFrom(pointInTime))
                {
                    yield return journalEntry;
                }

                SetState(preState);    
            }         
        }

        public void Dispose()
        {
            Close();
        }
    }
}
