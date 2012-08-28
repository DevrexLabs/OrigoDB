using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LiveDomain.Core.Configuration;
using LiveDomain.Core.Logging;

namespace LiveDomain.Core
{
	internal class CommandJournal : ICommandJournal
	{
        enum JournalState
        {
            Closed,
            Open
        }

        private IJournalWriter _writer;
		private IStorage _storage;
        private JournalState _state;
        private EngineConfiguration _engineConfig;
	    private static ILog _log = EngineConfiguration.Current.GetLogFactory().GetLogForCallingType();


        public CommandJournal(EngineConfiguration engineConfig, IStorage storage)
        {
            _engineConfig = engineConfig;
            _storage = storage;
        }



        public void CreateNextSegment()
        {
            if (_state == JournalState.Closed) throw new InvalidOperationException("Cant create journal segment when journal is closed");
            _log.Info("NewJournalSegment");
            _writer.Dispose();

            Stream stream = _storage.CreateJournalWriterStream(JournalWriterCreateOptions.NextSegment);
            _writer = _engineConfig.CreateJournalWriter(stream);
        }

        public IEnumerable<JournalEntry<Command>> GetEntriesFrom(JournalSegmentInfo segment)
        {
            JournalState preState = _state;
            SetState(JournalState.Closed);

            foreach (var journalEntry in _storage.GetJournalEntries(segment))
            {
                yield return journalEntry;
            }

            SetState(preState);			
        }

		public IEnumerable<JournalEntry<Command>> GetAllEntries()
		{
            return GetEntriesFrom(JournalSegmentInfo.Initial);
		}


		public void Open()
		{

            if (_state == JournalState.Open)
            {
                throw new InvalidOperationException("Can't open command journal, already open");
            }
            
            Stream stream = _storage.CreateJournalWriterStream(JournalWriterCreateOptions.Append);
            _writer = _engineConfig.CreateJournalWriter(stream);
            _state = JournalState.Open;
		}

		public void Append(Command command)
		{
            if (_state != JournalState.Open)
            {
                throw new InvalidOperationException("Can't append to journal when closed");
            }

			var entry = new JournalEntry<Command>(command);
			_writer.Write(entry);
            if (_writer.Length >= _engineConfig.JournalSegmentSizeInBytes)
            {
                CreateNextSegment();
            }
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
    }
}
