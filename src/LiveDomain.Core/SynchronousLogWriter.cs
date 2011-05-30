using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LiveDomain.Core
{
	internal class SynchronousLogWriter : IDisposable
	{
		Serializer _serializer;
        Stream _stream;

		internal SynchronousLogWriter(string file, Serializer serializer)
		{
			_stream = File.Open(file, FileMode.Append, FileAccess.Write);
			_serializer = serializer;
		}

		internal void Write(LogItem item)
		{
            _serializer.Write(item, _stream);
            _stream.Flush();
		}

		public void Dispose()
		{
			_stream.Dispose();
		}
	}
}
