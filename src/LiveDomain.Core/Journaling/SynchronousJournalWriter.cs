using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace LiveDomain.Core
{

    /// <summary>
    /// A writer that waits for the disk write to complete before returning. Slower but reliable.
    /// </summary>
	internal class SynchronousJournalWriter : JournalWriter
	{
        ISerializer _serializer;

        public SynchronousJournalWriter(Stream stream, ISerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (stream == null) throw new ArgumentNullException("stream");

            _serializer = serializer;
            _stream = stream;
        }
		public override void Write(JournalEntry item)
		{
            _serializer.Write(item, _stream);
            _stream.Flush();
		}

		public override void Close()
		{
			if(_stream.CanRead || _stream.CanWrite)
				_stream.Close();
		}
	}
}
