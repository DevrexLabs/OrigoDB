using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    internal class AsynchronousLogWriter : LogWriter, ILogWriter
    {

        Queue<LogItem> queue = new Queue<LogItem>();

        public void Write(LogItem logItem)
        {
            lock (queue)
            {
                queue.Enqueue(logItem);
            }
        }

    }
}
