using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LiveDomain.Core
{
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
