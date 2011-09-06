using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LiveDomain.Core
{
    internal abstract class JournalWriter
    {

        protected Serializer _serializer;
        protected Stream _stream;

        public virtual void Dispose()
        {
            if (_stream != null)
            {
                if (_stream.CanWrite) _stream.Flush();
                _stream.Dispose();
            }
        }

    }
}
