using System;
using System.Threading;
using System.Collections.Concurrent;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core
{

    /// <summary>
    /// The actual write to disk is performed by a background thread. The call to Write() adds to a queue and then returns immediately.
    /// <remarks>Faster response times with a risk of dataloss. Also has the ability to buffer commands when request rates burst</remarks>
    /// </summary>
	internal class AsynchronousJournalWriter : IJournalWriter
	{
        //TODO: Add some fault tolerance, exception handling, and engine notification so it can choose to shutdown if the journal isn't working.
        readonly AutoResetEvent _closeWaitHandle = new AutoResetEvent(false);
		BlockingCollection<JournalEntry> _queue;
        readonly IJournalWriter _decoratedWriter;
		Thread _writerThread;

		public AsynchronousJournalWriter(IJournalWriter writer)
		{
			_decoratedWriter = writer;
            Init();
		}

        private void Init()
        {
            _writerThread = new Thread(WriteBackground) { IsBackground = false };
            _queue = new BlockingCollection<JournalEntry>(new ConcurrentQueue<JournalEntry>());
            _writerThread.Start();
        }

		public void Write(JournalEntry item)
		{
			_queue.Add(item);
		}

		public void Close()
		{
			if (_queue.IsAddingCompleted) return;

			_queue.CompleteAdding();
			_closeWaitHandle.WaitOne();
			_decoratedWriter.Close();
		}

    	    void IDisposable.Dispose()
		{
			Close();
			_decoratedWriter.Dispose();
		}


		void WriteBackground()
		{
			_closeWaitHandle.Reset();
			while (!_queue.IsCompleted)
			{
				JournalEntry item;
				if (_queue.TryTake(out item, Timeout.Infinite)) _decoratedWriter.Write(item);
			}
			_closeWaitHandle.Set();
		}


        public void Handle(SnapshotCreated snapshotCreated)
        {
            //flush the queue
            _queue.CompleteAdding();
            _writerThread.Join();

            //notify actual writer
            _decoratedWriter.Handle(snapshotCreated);

            //restart 
            Init();
            
        }
    }
}