using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using OrigoDB.Core.Logging;
using OrigoDB.Core.Security;

namespace OrigoDB.Core
{
    /// <summary>
    /// Engine is responsible for executing commands and queries against
    /// the model while conforming to ACID.
    /// </summary>
    public partial class Engine : IDisposable
    {

        public event EventHandler<CommandExecutingEventArgs> BeforeExecute = delegate { };
        public event EventHandler<CommandExecutedEventArgs> AfterExecute = delegate { };

        readonly Stopwatch _executionTimer = new Stopwatch();
        static readonly ILogger _log = LogProvider.Factory.GetLoggerForCallingType();

        readonly EngineConfiguration _config;
        readonly IAuthorizer<Type> _authorizer;

        readonly IStore _store;
        readonly ISynchronizer _synchronizer;

        private readonly object _commandSequenceLock = new object();

        Kernel _kernel;
        bool _isDisposed = false;

        public EngineConfiguration Config { get { return _config; } }

        protected Engine(IStore store, EngineConfiguration config)
        {
            _config = config;
            _store = store;

            _synchronizer = _config.CreateSynchronizer();
            _authorizer = _config.CreateAuthorizer();

            store.Load();
            Restore();

            if (_config.SnapshotBehavior == SnapshotBehavior.AfterRestore)
            {
                _log.Info("Starting snaphot job on threadpool");

                ThreadPool.QueueUserWorkItem((o) => CreateSnapshot());

                //Give the snapshot thread a chance to start and aquire the readlock
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }

            Core.Config.Engines.AddEngine(config.Location.OfJournal, this);
        }

        private void Restore()
        {
            Model model = _store.LoadModel();
            _kernel = _config.CreateKernel(model);
            _kernel.SetSynchronizer(_synchronizer);

        }

        public object Execute(Query query)
        {
            return Execute<Model, object>(query.ExecuteStub);
        }

        public TResult Execute<TModel, TResult>(Func<TModel, TResult> lambdaQuery) where TModel : Model
        {
            ThrowIfDisposed();
            ThrowUnlessAuthenticated(lambdaQuery.GetType());
            return ExecuteQuery(new DelegateQuery<TModel, TResult>(lambdaQuery));
        }

        public TRresult Execute<TModel, TRresult>(Query<TModel, TRresult> query) where TModel : Model
        {
            ThrowIfDisposed();
            ThrowUnlessAuthenticated(query.GetType());
            return ExecuteQuery(query);
        }

        private TResult ExecuteQuery<TModel, TResult>(Query<TModel, TResult> query) where TModel : Model
        {
            return _kernel.ExecuteQuery(query);
        }



        public object Execute(Command command)
        {
            ThrowIfDisposed();
            ThrowUnlessAuthenticated(command.GetType());

            var eventArgs = new CommandExecutingEventArgs(command);
            BeforeExecute.Invoke(this, eventArgs);
            if (eventArgs.Cancel) throw new CommandAbortedException("Command canceled by event handler");
            lock (_commandSequenceLock)
            {
                DateTime start = DateTime.Now;
                _executionTimer.Restart();

                _store.AppendCommand(command);
                int lastEntryId = _store.LastEntryId;

                try
                {
                    return _kernel.ExecuteCommand(command);
                }
                catch (Exception ex)
                {
                    _store.InvalidatePreviousCommand();
                    if (!(ex is CommandAbortedException)) Rollback();
                    throw;
                }
                finally
                {
                    AfterExecute.Invoke(this, new CommandExecutedEventArgs(lastEntryId, command, start, _executionTimer.Elapsed));
                    _synchronizer.Exit();
                }
            }
        }

        private void ThrowUnlessAuthenticated(Type operationType)
        {
            if (!_authorizer.Allows(operationType, Thread.CurrentPrincipal))
            {
                var msg = String.Format("Access denied to type {0}", operationType);
                throw new UnauthorizedAccessException(msg);
            }
        }

        internal byte[] GetSnapshot()
        {
            var memoryStream = new MemoryStream();
            WriteSnapshotToStream(memoryStream);
            return memoryStream.GetBuffer();
        }

        internal void WriteSnapshotToStream(Stream stream)
        {
            var serializer = _config.CreateSerializer();
            _kernel.Read(model => serializer.Write(model, stream));
        }

        /// <summary>
        /// Writes a snapshot to the <see cref="IStore"/>
        /// </summary>
        public void CreateSnapshot()
        {
            _kernel.Read(model => _store.WriteSnapshot(model));
        }

        private void Rollback()
        {
            //release memory held by the corrupted model
            _kernel = null;
            GC.Collect();
            GC.WaitForFullGCComplete();

            Restore();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                Core.Config.Engines.Remove(this);
                if (_config.SnapshotBehavior == SnapshotBehavior.OnShutdown) CreateSnapshot();
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }


        ~Engine()
        {
            Dispose(false);
            Close();
        }

        /// <summary>
        /// Shuts down the engine
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().FullName);
        }


        #region Static non-generic Load and Create methods

        public static Engine Load(string location)
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location; //TODO: this smells
            return Load(config);
        }

        public static Engine Load(EngineConfiguration config)
        {
            if (!config.Location.HasJournal) throw new InvalidOperationException("Specify location to load from in non-generic load");
            var store = config.CreateStore();
            return new Engine(store, config);
        }

        public static Engine Create(Model model, string location)
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return Create(model, config);
        }

        public static Engine Create(Model model, EngineConfiguration config = null)
        {

            config = config ?? EngineConfiguration.Create();
            if (!config.Location.HasJournal) config.Location.SetLocationFromType(model.GetType());
            return Create<Model>(model, config);

        }


        #endregion

        #region Static generic Load methods

        /// <summary>
        /// Load from location using the default EngineConfiguration
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Engine<TModel> Load<TModel>(string location) where TModel : Model
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return Load<TModel>(config);
        }

        /// <summary>
        /// Load using an explicit configuration.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Engine<TModel> Load<TModel>(EngineConfiguration config = null) where TModel : Model
        {
            config = config ?? EngineConfiguration.Create();
            if (!config.Location.HasJournal) config.Location.SetLocationFromType<TModel>();
            var store = config.CreateStore();
            return new Engine<TModel>(store, config);
        }
        #endregion

        #region Generic Create methods

        public static Engine<TModel> Create<TModel>(string location) where TModel : Model, new()
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return Create<TModel>(config);
        }

        public static Engine<TModel> Create<TModel>(EngineConfiguration config = null) where TModel : Model, new()
        {
            return Create(new TModel(), config);
        }

        public static Engine<TModel> Create<TModel>(TModel model, EngineConfiguration config = null) where TModel : Model
        {
            config = config ?? EngineConfiguration.Create();
            if (!config.Location.HasJournal) config.Location.SetLocationFromType<TModel>();
            IStore store = config.CreateStore();
            store.Create(model);
            return Load<TModel>(config);
        }

        #endregion

        #region Static generic LoadOrCreate methods


        public static Engine<TModel> LoadOrCreate<TModel>(string location) where TModel : Model, new()
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return LoadOrCreate<TModel>(config);
        }

        public static Engine<TModel> LoadOrCreate<TModel>(EngineConfiguration config = null) where TModel : Model, new()
        {
            config = config ?? EngineConfiguration.Create();
            if (!config.Location.HasJournal) config.Location.SetLocationFromType<TModel>();
            Engine<TModel> result = null;

            var store = config.CreateStore();

            if (store.Exists)
            {
                result = Load<TModel>(config);
                _log.Debug("Engine Loaded");
            }
            else
            {
                result = Create(new TModel(), config);
                _log.Debug("Engine Created");
            }
            return result;
        }

        #endregion
    }
}
