using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Collections.Concurrent;

namespace LiveDomain.Core
{


    
    internal class AsynchronousLogWriter : ILogWriter
    {
        
        SynchronousLogWriter _wrappedWriter;
        BlockingCollection<LogItem> _queue;
        Thread _writerThread;

        public AsynchronousLogWriter(SynchronousLogWriter writer)
        {
            _queue = new BlockingCollection<LogItem>(new ConcurrentQueue<LogItem>());
            _wrappedWriter = writer;
            _writerThread = new Thread(WriteBackground) { IsBackground = false };
            _writerThread.Start();
        }

        

        public void Write(LogItem logItem)
        {
            _queue.Add(logItem);
        }


        private void WriteBackground()
        {
            while (!_queue.IsCompleted)
            {
                try
                {
                    LogItem logItem = _queue.Take();
                    _wrappedWriter.Write(logItem);
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }

            }
            _wrappedWriter.Dispose();
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _writerThread.Interrupt();
            
        }

    }

}
