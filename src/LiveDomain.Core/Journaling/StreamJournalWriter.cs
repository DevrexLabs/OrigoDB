using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using LiveDomain.Core.Logging;

namespace LiveDomain.Core
{

    /// <summary>
    /// Serializes journal entries to a stream, flushing after each write
    /// </summary>
	internal class StreamJournalWriter : IJournalWriter
	{
        ISerializer _serializer;
        Stream _stream;
        private EngineConfiguration _config;
        private FileStore _storage;
        private RolloverStrategy _rolloverStrategy;
        private long _entriesWrittenToCurrentStream;

        private static ILog _log = LogProvider.Factory.GetLogForCallingType();




        public virtual void Dispose()
        {
            if (_stream != null)
            {
                if (_stream.CanWrite) _stream.Flush();
                _stream.Dispose();
            }
        }

        public StreamJournalWriter(FileStore storage, EngineConfiguration config)
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
				_stream = _storage.CreateJournalWriterStream(item.Id + 1);
				_entriesWrittenToCurrentStream = 0;
			}
			
            _serializer.Write(item, _stream);
            _stream.Flush();
			_entriesWrittenToCurrentStream++;
		}

		public void Close()
		{
			if(_stream != null && (_stream.CanRead || _stream.CanWrite))
				_stream.Close();
		}
	}
}
