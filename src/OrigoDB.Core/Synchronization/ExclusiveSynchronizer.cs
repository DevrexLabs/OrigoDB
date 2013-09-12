using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OrigoDB.Core
{

    /// <summary>
    /// Single thread at a time for either reading or writing. 
    /// Use for baseline performance testing to be compared with concurrent reading
    /// </summary>
    public class ExclusiveSynchronizer : ISynchronizer
    {
        object _lock = new object();

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
