using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    public enum JournalWriterPerformanceMode
    {
        /// <summary>
        /// Safe but slower
        /// Journal writer waits for disc write and flush, exceptions are propagated to client.
        /// </summary>
        Synchronous,

        /// <summary>
        /// High Performance but unsafe, commands can get lost.
        /// Journal writer runs in the background, disk write time is not included in command execution time.
        /// </summary>
        Asynchronous
    }
}
