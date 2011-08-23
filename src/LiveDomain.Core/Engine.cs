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


    /// <summary>
    /// Engine is responsible for executing commands and queries against
    /// the model while conforming to ACID.
    /// </summary>
    /// <typeparam name="M"></typeparam>
    public class Engine : IDisposable
    {

        public static Engine Load(EngineSettings settings)
        {
            Model model = Restore(settings);
            return new Engine( model, settings);
        }

        public static void Create( Model model, EngineSettings settings)
        {
            var storage = Storage.Create(settings); 
            storage.Merge(model);
            storage.Dispose();
        }

        /// <summary>
        /// The prevalent system. The database. Your single aggregate root.
        /// </summary>
        protected Model _model;

        /// <summary>
        /// The time to wait for the lock before a TimeoutException is thrown.
        /// </summary>
        public TimeSpan LockTimeout { get; set; }


        /// <summary>
        /// Set to true to avoid returning references directly into the model. Default is true.
        /// </summary>
        /// 
        public bool CloneResults { get; set; }



        Storage _storage;
        ILockStrategy _lock;
        Serializer _serializer;
        bool _isDisposed = false;
        EngineSettings _settings;



        /// <summary>
        /// Shuts down the server
        /// </summary>
        public void Close()
        {
            if (!_isDisposed)
            {
                _lock.EnterWrite();
                DisposeImpl();
                _isDisposed = true;
            }
        }

        private void DisposeImpl()
        {
            _storage.Dispose();
            _model = null;
        }

        protected static Model Restore(EngineSettings settings)
        {
            Storage storage = new Storage(settings);
            Model model = storage.ReadModel();
            model.OnLoad();

            foreach (var logitem in storage.ReadLogEntries())
            {
                logitem._command.Redo(model);
            }
            storage.Dispose();
            return model;
        }

        /// <summary>
        /// Merge committed transactions with the current image on disk and empty the transaction log.
        /// </summary>
        public void Merge()
        {
            ThrowIfDisposed();
            try
            {
                _lock.EnterRead();
                _storage.Merge(_model);
            }
            finally
            {
                _lock.Exit();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new InvalidOperationException("Engine has been disposed");
        }



        protected Engine(Model model, EngineSettings settings)
        {
            _settings = settings;
            _storage = new Storage(_settings);
            _model = model;
            _lock = _settings.CreateLockingStrategy();
            _serializer = new Serializer(_settings.CreateSerializationFormatter());
        }


        public object Execute(Query query)
        {
            return Execute(query, CloneResults);
        }

        public object Execute(Query query, bool cloneResults)
        {
            return Execute<Model, object>((m) => query.ExecuteStub(m), cloneResults);
        }

        public T Execute<M, T>(Func<M, T> query) where M : Model
        {
            return Execute(query, CloneResults);
        }

        public T Execute<M,T>(Func<M, T> query, bool cloneResults) where M : Model
        {
            ThrowIfDisposed();
            _lock.EnterRead();

            try
            {
                T result = query.Invoke(_model as M);
                if (cloneResults) result = _serializer.Clone(result);
                return result;
            }
            finally
            {
                _lock.Exit();
            }

        }

        public object Execute(Command command)
        {
            return Execute(command, CloneResults);
        }

        public object Execute(Command command, bool cloneResults)
        {
            ThrowIfDisposed();

            _lock.EnterUpgrade();
            try
            {
                CallPrepare(command);
                _lock.EnterWrite();
                object result = command.ExecuteStub(_model);
                _storage.AppendToLog(command);
                if (cloneResults) result = _serializer.Clone(result);
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
                _lock.Exit();
            }
        }

        private void RollBack()
        {
            try
            {
                string path = _storage.RootDirectory;
                _storage.Dispose();
                _model = Restore( _settings);
                _storage = new Storage( _settings);
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


        /// <summary>
        /// Discards the commands in the transaction log and restores from latest Merge()
        /// </summary>
        public void RevertToSnapshot()
        {
            ThrowIfDisposed();
            _storage.TruncateLog();
            _model = _storage.ReadModel();
        }

        public void Dispose()
        {
            this.Close();
        }


        public static Engine Load(string Path)
        {
            return Load(new EngineSettings(Path));
        }
    }

    public class Engine<M> : Engine where M : Model
    {
        public T Execute<T>(CommandWithResult<M, T> command)
        {
            return (T) base.Execute(command);
        }

        public void Execute(Command<M> command)
        {
            base.Execute(command);
        }

        public T Execute<T>(Query<M, T> query)
        {
            return (T) base.Execute(query);
        }

        internal Engine(string path, M model) : this(model, new EngineSettings(path)) { }
        internal Engine(M model, EngineSettings settings) : base(model, settings)
        {
        }
    }
}
