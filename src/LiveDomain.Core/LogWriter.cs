using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LiveDomain.Core
{
    internal abstract class LogWriter
    {

        protected internal Serializer Serializer { get; set; }
        protected internal Stream Stream { get; set; }

        public void Dispose()
        {
            Stream.Flush();
            Stream.Dispose();
        }

    }
}
