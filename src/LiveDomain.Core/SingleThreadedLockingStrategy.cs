using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LiveDomain.Core
{

    /// <summary>
    /// Single thread at a time for either reading or writing. 
    /// Use for baseline performance testing to be compared with concurrent reading
    /// </summary>
    public class SingleThreadedLockingStrategy : ILockStrategy
    {
        object _lock = new object();

        public SingleThreadedLockingStrategy(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public TimeSpan Timeout { get; set; }

        public void EnterRead()
        {
            Monitor.TryEnter(_lock, Timeout);
        }

        public void EnterUpgrade()
        {
            Monitor.TryEnter(_lock, Timeout);
        }

        public void EnterWrite()
        {
            Monitor.TryEnter(_lock, Timeout);
        }

        public void Exit()
        {
            Monitor.Exit(_lock);
        }

    }
}
