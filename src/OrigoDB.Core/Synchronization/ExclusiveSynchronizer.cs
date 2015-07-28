using System;
using System.Threading;

namespace OrigoDB.Core
{

    /// <summary>
    /// ISychronizer implementation allowing a single reader or writer at any given time.
    /// For some workloads, this simpler lock may outperform ReadWriteSynchronizer. Measure!
    /// </summary>
    public class ExclusiveSynchronizer : ISynchronizer
    {
        readonly object _lock = new object();

        public ExclusiveSynchronizer(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public TimeSpan Timeout { get; set; }

        public void EnterRead()
        {
            Lock();
        }

        public void EnterUpgrade()
        {
            Lock();
        }

        public void EnterWrite()
        {
            Lock();
        }

        public void Exit()
        {
            Monitor.Exit(_lock);
        }

        private void Lock()
        {
            if(!Monitor.TryEnter(_lock, Timeout)) throw new TimeoutException("Couldn't aquire lock within the specified timeout period");
        }
    }
}
