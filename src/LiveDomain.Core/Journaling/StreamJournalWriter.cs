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
        private FileStorage _storage;
        private RolloverStrategy _rolloverStrategy;
        private static ILog _log = Log.GetLogFactory().GetLogForCallingType();

        private long _entriesWritten = 0;


        public virtual void Dispose()
        {
            if (_stream != null)
            {
                if (_stream.CanWrite) _stream.Flush();
                _stream.Dispose();
            }
        }

        public StreamJournalWriter(FileStorage storage, Stream stream, EngineConfiguration config, RolloverStrategy rolloverStrategy)
        {
            _config = config;
            _storage = storage;
            _serializer = _config.CreateSerializer();
            _stream = stream;
            _rolloverStrategy = rolloverStrategy;
        }

		public void Write(JournalEntry item)
		{
            _serializer.Write(item, _stream);
            _stream.Flush();

		    if (_rolloverStrategy.Rollover(_stream.Position, _entriesWritten))
		    {
                _log.Debug("NewJournalSegment");
		        Close();
                _stream = _storage.CreateJournalWriterStream(item.Id + 1);
		    }

		}

		public void Close()
		{
			if(_stream.CanRead || _stream.CanWrite)
				_stream.Close();
		}
	}
}
