using System.IO;
using OrigoDB.Core.Logging;

namespace OrigoDB.Core
{
    /// <summary>
    /// Serializes journal entries to a stream, flushing after each write
    /// </summary>
	internal class StreamJournalWriter : IJournalWriter
	{
        ISerializer _serializer;
        Stream _stream;
        private EngineConfiguration _config;

        private IStore _storage;
        private RolloverStrategy _rolloverStrategy;
        private long _entriesWrittenToCurrentStream;

        private static ILogger _log = LogProvider.Factory.GetLoggerForCallingType();




        public virtual void Dispose()
        {
            if (_stream != null)
            {
                if (_stream.CanWrite) _stream.Flush();
                _stream.Dispose();
                _stream = null;
            }
        }

        public StreamJournalWriter(IStore storage, EngineConfiguration config)
        {
            _config = config;
            _storage = storage;
            _serializer = _config.CreateSerializer();
            _rolloverStrategy = _config.CreateRolloverStrategy();
        }

		public void Write(JournalEntry item)
		{
			if (_stream == null) _stream = _storage.CreateJournalWriterStream(item.Id);
			if (_rolloverStrategy.Rollover(_stream.Position, _entriesWrittenToCurrentStream))
			{
				_log.Debug("NewJournalSegment");
				Close();
				_stream = _storage.CreateJournalWriterStream(item.Id);
				_entriesWrittenToCurrentStream = 0;
			}
			
            _serializer.Write(item, _stream);
            _stream.Flush();
			_entriesWrittenToCurrentStream++;
		}

		public void Close()
		{
		    Dispose();
		}
	}
}
