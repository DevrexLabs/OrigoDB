using System;
using System.Threading;

namespace OrigoDB.Core
{

    /// <summary>
    /// The default ISynchronizer implementation supporting a single writer or multiple readers.
    /// </summary>
    public class ReadWriteSynchronizer : ISynchronizer
    {

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        
        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);


        public TimeSpan Timeout { get; set; }


        public ReadWriteSynchronizer() : this(DefaultTimeout)
        {
            
        }
        public ReadWriteSynchronizer(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public void EnterRead()
        {
            if (!_lock.TryEnterReadLock(Timeout))
            {
                throw new TimeoutException("no read lock aquired within timeout period");
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
