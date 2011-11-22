using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LiveDomain.Core
{
    internal abstract class JournalWriter : IJournalWriter
    {
        protected Stream _stream;

        public long Length 
        { 
            get 
            { 
                return _stream.Position; 
            }
        }

        public virtual void Dispose()
        {
            if (_stream != null)
            {
                if (_stream.CanWrite) _stream.Flush();
                _stream.Dispose();
            }
        }



        public abstract void Write(JournalEntry item);
        public abstract void Close();
    }
}
