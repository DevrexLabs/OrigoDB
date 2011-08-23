using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LiveDomain.Core
{

    /// <summary>
    /// Wrapper for System.Threading.ReaderWriterLockSlim class
    /// </summary>
    public class ReaderWriterLockSlimStrategy : ILockStrategy
    {

        
        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);


        public TimeSpan Timeout { get; set; }

        public ReaderWriterLockSlimStrategy(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public void EnterRead()
        {
            if (!_lock.TryEnterReadLock(Timeout))
            {
                throw new TimeoutException("no upgrade lock aquired within timeout period");
            }
        }

        public void EnterUpgrade()
        {
            if (!_lock.TryEnterUpgradeableReadLock(Timeout))
            {
                throw new TimeoutException("no upgrade lock aquired within timeout period");
            }            
        }

        public void EnterWrite()
        {
            if (!_lock.TryEnterWriteLock(Timeout))
            {
                throw new TimeoutException("no write lock aquired within timeout period");
            }
        }


        public void Exit()
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            if (_lock.IsUpgradeableReadLockHeld) _lock.ExitUpgradeableReadLock();
            if (_lock.IsReadLockHeld) _lock.ExitReadLock();
        }
    }
}
