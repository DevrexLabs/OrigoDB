using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace LiveDomain.Core
{
	internal class SynchronousLogWriter : LogWriter, ILogWriter
	{
        public SynchronousLogWriter(Stream stream, Serializer serializer)
        {
            _serializer = serializer;
            _stream = stream;
        }
		public void Write(LogItem item)
		{
            _serializer.Write(item, _stream);
            _stream.Flush();
		}
	}
}
