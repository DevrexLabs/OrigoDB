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

    /// <summary>
    /// The actual write to disk is performed by a background thread. The call to Write() adds to a queue and then returns immediately.
    /// <remarks>Faster response times with a risk of dataloss. Also has the ability to buffer commands when request rates burst</remarks>
    /// </summary>
	internal class AsynchronousLogWriter : ILogWriter
	{
        //TODO: Add some fault tolerance, exception handling, and engine notification so it can choose to shutdown if the log isnt working.
		AutoResetEvent _closeWaitHandle = new AutoResetEvent(false);
		BlockingCollection<LogItem> _queue;
		SynchronousLogWriter _wrappedWriter;
		Thread _writerThread;

		public AsynchronousLogWriter(SynchronousLogWriter writer)
		{
			_wrappedWriter = writer;
			_writerThread = new Thread(WriteBackground) {IsBackground = false};
			_queue = new BlockingCollection<LogItem>(new ConcurrentQueue<LogItem>());
			_writerThread.Start();
		}

		#region ILogWriter Members

		public void Write(LogItem logItem)
		{
			_queue.Add(logItem);
		}

		public void Close()
		{
			if (_queue.IsAddingCompleted) return;

			_queue.CompleteAdding();
			_closeWaitHandle.WaitOne();
			_wrappedWriter.Close();
		}

		public void Dispose()
		{
			Close();
			_wrappedWriter.Dispose();
		}

		#endregion

		void WriteBackground()
		{
			_closeWaitHandle.Reset();
			while (!_queue.IsCompleted)
			{
				LogItem logItem;
				if (_queue.TryTake(out logItem, Timeout.Infinite)) _wrappedWriter.Write(logItem);
			}
			_closeWaitHandle.Set();
		}
	}
}