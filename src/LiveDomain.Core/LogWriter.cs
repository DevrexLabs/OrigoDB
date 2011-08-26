using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LiveDomain.Core
{
    internal abstract class LogWriter
    {

        protected Serializer _serializer;
        protected Stream _stream;

        public virtual void Dispose()
        {
            _stream.Flush();
            _stream.Dispose();
        }

    }
}
