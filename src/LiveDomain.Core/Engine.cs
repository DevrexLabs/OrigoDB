using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Diagnostics;

namespace LiveDomain.Core
{


    public abstract class Engine
    {

        protected static M Recover<M>(string directory) where M : Model
        {
            Storage storage = new Storage(directory);
            M model = storage.ReadModel<M>();

            foreach (var logitem in storage.ReadLogEntries())
            {
                logitem._command.Redo(model);
            }
            storage.Dispose();
            return model;
        }

        public static Engine<M> Load<M>(string directory) where M : Model
        {
            M model = Recover<M>(directory);
            return new Engine<M>(directory, model);
        }

        public static void Create<M>(string directory, M model) where M : Model
        {
            //TODO: Refactor, responsibility of Storage
            if (Directory.Exists(directory)) throw new Exception("Directory already exists");
            Directory.CreateDirectory(directory);
            //Directory.CreateDirectory(directory + @"\snapshots");
            var storage = new Storage(directory);
            storage.Merge(model);
            storage.Dispose();
            //return Engine.Load<M>(directory);
        }
    }

    /// <summary>
    /// Engine is responsible for executing commands and queries against
    /// the model while conforming to ACID.
    /// </summary>
    /// <typeparam name="M"></typeparam>
    public class Engine<M> : Engine, IDisposable where M : Model
    {

        /// <summary>
        /// The time to wait for the lock before a TimeoutException is thrown.
        /// </summary>
        public TimeSpan LockTimeout { get; set; }


        /// <summary>
        /// The encapsulated prevalent system
        /// </summary>
        M _model;
        Storage _storage;
        ReaderWriterLockSlim _lock;
        Serializer _serializer;
        bool _isDisposed = false;


        /// <summary>
        /// Shuts down the server
        /// </summary>
        public void Close()
        {
            if (!_isDisposed)
            {
                EnterWriteLockOrDie();
                //Forces close of the command log
                _isDisposed = true;
            }
        }

        private void DisposeImpl()
        {
            _storage.Dispose();
            _model = null;
        }


        /// <summary>
        /// Merge committed transactions with the current image on disk and empty the transaction log.
        /// </summary>
        public void Merge()
        {
            ThrowIfDisposed();
            try
            {
                EnterReadLockOrDie();
                _storage.Merge(_model);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new InvalidOperationException("Engine has been disposed");
        }



        internal Engine(string directory, M model)
        {
            _storage = new Storage(directory);
            _model = model;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _serializer = new Serializer(new BinaryFormatter());
            LockTimeout = TimeSpan.FromSeconds(2);
        }



        public T Execute<T>(Func<M, T> query, bool cloneResults = true)
        {
            ThrowIfDisposed();
            EnterReadLockOrDie();

            try
            {
                T result = query.Invoke(_model);
                if (cloneResults) result = _serializer.Clone<T>(result);
                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public T Execute<T>(CommandWithResult<M, T> command)
        {
            return ExecuteImpl<T>(command);
        }

        public void Execute(Command<M> command)
        {
            ExecuteImpl<object>(command);
        }

        private T ExecuteImpl<T>(Command command)
        {
            ThrowIfDisposed();

            EnterUpgradeLockOrDie();
            try
            {
                CallPrepare(command);
                EnterWriteLockOrDie();
                T result = default(T);
                if (command is CommandWithResult<M, T>) result = (command as CommandWithResult<M, T>).Execute(_model);
                else command.ExecuteStub(_model);
                _storage.AppendToLog(command);
                return result;
            }
            catch (TimeoutException) { throw; }
            catch (CommandFailedException) { throw; }
            catch (FatalException) { DisposeImpl(); throw; }
            catch (Exception ex) 
            {
                RollBack();
                throw new CommandFailedException("Command threw an exception, Rollback() was performed, see inner exception for details", ex);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
                if (_lock.IsUpgradeableReadLockHeld) _lock.ExitUpgradeableReadLock();
            }
        }

        private void RollBack()
        {
            try
            {
                string path = _storage.RootDirectory;
                _storage.Dispose();
                _model = Recover<M>(path);
                _storage = new Storage(path);
            }
            catch (Exception ex)
            {
                Dispose();
                throw new FatalException("Rollback failed, shutting down. See inner exception for details", ex);
            }
        }

        private void CallPrepare(Command command)
        {
            try
            {
                command.PrepareStub(_model);
            }
            catch (Exception ex)
            {

                throw new CommandFailedException("Exception thrown during Prepare(), see inner exception for details", ex);
            }
        }


        private void EnterWriteLockOrDie()
        {
            if (!_lock.TryEnterWriteLock(LockTimeout))
            {
                throw new TimeoutException("no write lock aquired within timeout period");
            }
        }


        private void EnterReadLockOrDie()
        {
            if (!_lock.TryEnterReadLock(LockTimeout))
            {
                throw new TimeoutException(" no read lock aquired within timeout period");
            }
        }

        private void EnterUpgradeLockOrDie()
        {
            if (!_lock.TryEnterUpgradeableReadLock(LockTimeout))
            {
                throw new TimeoutException("no upgradeable read lock aquired within timeout period");
            }
        }

        /// <summary>
        /// Discards the commands in the transaction log and restores from latest Merge()
        /// </summary>
        public void Revert()
        {
            ThrowIfDisposed();
            _storage.TruncateLog();
            _model = _storage.ReadModel<M>();
        }

        public void Dispose()
        {
            this.Close();
        }

    }
}
