using System;
using System.Linq;
using LiveDomain.Core.Utilities;
using LiveDomain.Core.Logging;

namespace LiveDomain.Core
{

    /// <summary>
    /// The kernel coordinates concurrent access to the
    /// model and executes command and queries
    /// </summary>
    public abstract class Kernel : IDisposable
    {

        private static ILog _log = LogProvider.Factory.GetLogForCallingType();

        protected Model _model;

        protected readonly EngineConfiguration _config;
        protected readonly ICommandJournal _commandJournal;
        protected readonly ISynchronizer _synchronizer;
        protected readonly ISerializer _serializer;
        protected readonly IStore _store;

        /// <summary>
        /// Apply the command to the model and save it to the journal, 
        /// not necessarily in that order
        /// </summary>
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
            _commandJournal = config.CreateCommandJournal(store);
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

        protected void Restore()
        {
            Restore(() => (Model)Activator.CreateInstance(_model.GetType()));
        }
        /// <summary>
        /// Restore the model from the latest snapshot and replay subsequent commands
        /// </summary>
        /// <param name="constructor">Used to create the initial model if there is no snapshot</param>
        public void Restore<M>(Func<M> constructor = null) where M : Model
        {

            constructor = constructor ?? Activator.CreateInstance<M>;

            long lastEntryIdExecuted;
            _model = _store.LoadMostRecentSnapshot(out lastEntryIdExecuted);

            if (_model == null) _model = constructor.Invoke();

            _model.SnapshotRestored();
            foreach (var command in _commandJournal.GetEntriesFrom(lastEntryIdExecuted).Select(entry => entry.Item))
            {
                command.Redo(_model);
            }
            _model.JournalRestored();
        }

        /// <summary>
        /// Provide synchronized read access to the model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readAction"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Provides synchronized read access to the model
        /// </summary>
        /// <param name="readAction"></param>
        public void Read(Action<Model> readAction)
        {
            Read((m) => {
                readAction.Invoke(m);
                return 0;
            });
        }

        public void Dispose()
        {
            _commandJournal.Dispose();
        }


        /// <summary>
        /// Writes a snapshot to the <see cref="IStore"/>
        /// </summary>
        internal void CreateSnapshot()
        {
            long lastEntryId = _commandJournal.LastEntryId;
            _log.Info("BeginSnapshot:" + lastEntryId);
            Read(m => _store.WriteSnapshot(m, lastEntryId));
            _log.Info("EndSnapshot:" + lastEntryId);

        }
    }
}
