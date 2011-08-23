using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LiveDomain.Core
{
	internal class SynchronousLogWriter : LogWriter, ILogWriter
	{

		public void Write(LogItem item)
		{
            Serializer.Write(item, Stream);
            Stream.Flush();
		}
	}
}
