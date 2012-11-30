using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core.Utilities;
using LiveDomain.Core.Logging;

namespace LiveDomain.Core
{

    /// <summary>
    /// The kernel coordinates concurrent access to the
    /// model and executes command and queries
    /// </summary>
    public abstract class Kernel
    {

        private static ILog _log = LogProvider.Factory.GetLogForCallingType();

        protected Model _model;

        protected readonly EngineConfiguration _config;
        protected readonly ICommandJournal _commandJournal;
        protected readonly ISynchronizer _synchronizer;
        protected readonly ISerializer _serializer;
        protected readonly IStore _store;

        public abstract object ExecuteCommand(Command command);

        public virtual T ExecuteQuery<M, T>(Query<M, T> query) where M : Model
        {
            try
            {
                _synchronizer.EnterRead();
                object result = query.ExecuteStub(_model as M);
                EnsureSafeResults(ref result, query);
                return (T)result;
            }
            catch (TimeoutException)
            {
                //ThrowIfDisposed();
                throw;
            }
            finally
            {
                _synchronizer.Exit();
            }            
        }

        protected Kernel(EngineConfiguration config, IStore store)
        {
            _config = config;
            _commandJournal = config.CreateCommandJournal();
            _synchronizer = _config.CreateSynchronizer();
            _serializer = config.CreateSerializer();
            _store = store;
        }

        /// <summary>
        /// Make sure we don't return direct references to mutable objects within the model
        /// </summary>
        protected void EnsureSafeResults(ref object graph, IOperationWithResult operation)
        {
            if (_config.EnsureSafeResults && graph != null)
            {
                bool operationIsResponsible = operation != null && operation.ResultIsSafe;

                if (!operationIsResponsible && !graph.IsImmutable())
                {
                    graph = _serializer.Clone(graph);
                }
            }
        }

        public void Restore<M>(Func<M> constructor) where M : Model
        {

            long lastEntryIdExecuted;
            _model = _store.LoadMostRecentSnapshot(out lastEntryIdExecuted);

            if (_model == null)
            {
                if (constructor == null) throw new ApplicationException("No initial snapshot");
                _model = constructor.Invoke();
            }

            _model.SnapshotRestored();
            foreach (var command in _commandJournal.GetEntriesFrom(lastEntryIdExecuted).Select(entry => entry.Item))
            {
                command.Redo(_model);
            }
            _model.JournalRestored();
        }

        public T Read<T>(Func<Model, T> readAction)
        {
            try
            {
                _synchronizer.EnterRead();
                return readAction.Invoke(_model);
            }
            finally
            {
                _synchronizer.Exit();
            }
        }

        public void Read(Action<Model> readAction)
        {
            try
            {
                _synchronizer.EnterRead();
                readAction.Invoke(_model);
            }
            finally
            {
                _synchronizer.Exit();
            }
        }

        public void Dispose()
        {
            _commandJournal.Dispose();
        }

        internal void CreateSnapshot()
        {
            long lastEntryId = _commandJournal.LastEntryId;
            _log.Info("BeginSnapshot:" + lastEntryId);
            Read(m => _store.WriteSnapshot(m, lastEntryId));
            _log.Info("EndSnapshot:" + lastEntryId);

        }
    }
}
