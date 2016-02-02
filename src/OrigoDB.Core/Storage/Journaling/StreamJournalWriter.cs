using System;
using System.IO;
using System.Runtime.Serialization;
using OrigoDB.Core.Logging;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core
{
    /// <summary>
    /// Serializes journal entries to a stream, flushing after each write
    /// </summary>
	internal class StreamJournalWriter : IJournalWriter
	{
        readonly IFormatter _journalFormatter;
        readonly RolloverStrategy _rolloverStrategy;
        readonly Func<ulong, Stream> _streamProvider;

        private Stream _stream;
        private long _entriesWrittenToCurrentStream;

        private static ILogger _log = LogProvider.Factory.GetLoggerForCallingType();


        public virtual void Dispose()
        {
            Close();
        }

        public StreamJournalWriter(EngineConfiguration config, Func<ulong,Stream> streamFactory)
        {
            _streamProvider = streamFactory;
            _journalFormatter = config.CreateFormatter(FormatterUsage.Journal);
            _rolloverStrategy = config.CreateRolloverStrategy();
        }

		public void Write(JournalEntry entry)
		{
			if (_stream == null) _stream = _streamProvider.Invoke(entry.Id);
			if (_rolloverStrategy.Rollover(_stream.Position, _entriesWrittenToCurrentStream))
			{
                Rollover(entry.Id);
			}
			
            _journalFormatter.WriteBuffered(_stream, entry);
            _stream.Flush();
			_entriesWrittenToCurrentStream++;
		}

		public void Close()
		{
            if (_stream != null)
            {
                if (_stream.CanWrite) _stream.Flush();
                _stream.Dispose();
                _stream = null;
            }
		}


        public void Handle(SnapshotCreated snapshotCreated)
        {
            Rollover(snapshotCreated.Revision + 1);
        }

        private void Rollover(ulong id)
        {
            _log.Debug("NewJournalSegment");
            Close();
            _stream = _streamProvider.Invoke(id);
            _entriesWrittenToCurrentStream = 0;
        }
    }
}
