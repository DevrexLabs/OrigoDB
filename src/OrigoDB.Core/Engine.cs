using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using OrigoDB.Core.Logging;
using OrigoDB.Core.Security;
using System.Runtime.Serialization;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core
{
    /// <summary>
    /// Engine is responsible for executing commands and queries against
    /// the model while conforming to ACID.
    /// </summary>
    public partial class Engine : IDisposable
    {

        /// <summary>
        /// Fired just before the command is executed
        /// </summary>
        public event EventHandler<CommandExecutingEventArgs> CommandExecuting = delegate { };

        /// <summary>
        /// Fired after the command has successfully executed
        /// </summary>
        public event EventHandler<CommandExecutedEventArgs> CommandExecuted = delegate { };

        readonly Stopwatch _executionTimer = new Stopwatch();
        static readonly ILogger _log = LogProvider.Factory.GetLoggerForCallingType();

        readonly EngineConfiguration _config;
        readonly IAuthorizer _authorizer;
        private JournalAppender _journalAppender;


        private ICommandStore _commandStore;
        private ISnapshotStore _snapshotStore;
        readonly ISynchronizer _synchronizer;

        readonly List<IEvent> _capturedEvents = new List<IEvent>(100);

        private readonly object _commandSequenceLock = new object();

        Kernel _kernel;
        bool _isDisposed;

        /// <summary>
        /// The current configuration
        /// </summary>
        public EngineConfiguration Config { get { return _config; } }

        protected Engine(Model model, EngineConfiguration config)
        {
            _config = config;

            _synchronizer = _config.CreateSynchronizer();
            _authorizer = _config.CreateAuthorizer();
            Configure(model);

            if (_config.SnapshotBehavior == SnapshotBehavior.AfterRestore)
            {
                _log.Info("Starting snaphot job on threadpool");

                ThreadPool.QueueUserWorkItem(_ => CreateSnapshot());

                //Give the snapshot thread a chance to start and aquire the readlock
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }

            CommandExecuted += (s, e) => HandleSnapshotPersistence();
            
            Core.Config.Engines.AddEngine(config.Location.OfJournal, this);
        }

        private void Restore()
        {
            var model = new ModelLoader(_config, _commandStore, _snapshotStore).LoadModel();
            Configure(model);
        }

        /// <summary>
        /// First time initialization and reconfig in case of restore
        /// </summary>
        /// <param name="model"></param>
        private void Configure(Model model)
        {
            _commandStore = _config.CreateCommandStore();
            _snapshotStore = _config.CreateSnapshotStore();
            _journalAppender = JournalAppender.Create(model.Revision + 1, _commandStore);
            _kernel = _config.CreateKernel(model);
            _kernel.SetSynchronizer(_synchronizer);

            //Capture events emitted during Command.Execute
            model.EventDispatcher.Subscribe( e => _capturedEvents.Add(e));
        }

        /// <summary>
        /// DANGER! Get a direct reference to the encapsulated model. DANGER!
        /// <remarks>
        /// Under normal circumstances you will never touch the model directly. Access is not thread safe and
        /// any changes will be lost unless a snapshot is taken.
        /// </remarks>
        /// </summary>
        /// <returns>A direct reference to the model</returns>
        public Model GetModel()
        {
            return _kernel.Model;
        }

        /// <summary>
        /// Non generic query execution overload
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public object Execute(Query query)
        {
            return Execute<Model, object>(query.ExecuteStub);
        }

        /// <summary>
        /// Execute a lambda query
        /// </summary>
        public TResult Execute<TModel, TResult>(Func<TModel, TResult> lambdaQuery) where TModel : Model
        {
            EnsureRunning();
            EnsureAuthorized(lambdaQuery);
            return ExecuteQuery(new DelegateQuery<TModel, TResult>(lambdaQuery));
        }

        /// <summary>
        /// Execute query overload
        /// </summary>
        public TRresult Execute<TModel, TRresult>(Query<TModel, TRresult> query) where TModel : Model
        {
            EnsureRunning();
            EnsureAuthorized(query);
            return ExecuteQuery(query);
        }

        /// <summary>
        /// Actual command execution
        /// </summary>
        public object Execute(Command command)
        {
            EnsureRunning();
            EnsureAuthorized(command);
            FireExecutingEvent(command);

            lock (_commandSequenceLock)
            {
                command.Timestamp = DateTime.Now;
                bool exceptionThrown = false;
                _executionTimer.Restart();
                ulong lastEntryId = PersistIfJournaling(command);
                try
                {
                    _capturedEvents.Clear();
                    return _kernel.ExecuteCommand(command);
                }
                catch (Exception ex)
                {
                    exceptionThrown = true;
                    if (_config.PersistenceMode == PersistenceMode.Journaling) _journalAppender.AppendRollbackMarker();
                    if (!(ex is CommandAbortedException))
                    {
                        Rollback();
                        ex = new CommandFailedException("Command failed with rollback, see inner exception for details", ex);
                    }
                    throw ex;
                }
                finally
                {
                    _synchronizer.Exit();
                    if (!exceptionThrown)
                    {
                        var args = new CommandExecutedEventArgs(lastEntryId, command, command.Timestamp, _executionTimer.Elapsed, _capturedEvents);
                        CommandExecuted.Invoke(this, args);
                    }
                }
            }
        }

        private void FireExecutingEvent(Command command)
        {
            var eventArgs = new CommandExecutingEventArgs(command);
            CommandExecuting.Invoke(this, eventArgs);
            if (eventArgs.Cancel) throw new CommandAbortedException("Command canceled by event handler");
        }

        private TResult ExecuteQuery<TModel, TResult>(Query<TModel, TResult> query) where TModel : Model
        {
            return _kernel.ExecuteQuery(query);
        }


        private ulong PersistIfJournaling(Command command)
        {
            return _config.PersistenceMode == PersistenceMode.Journaling
                ? _journalAppender.Append(command)
                : 0;
        }

        private void HandleSnapshotPersistence()
        {
            if (_config.PersistenceMode == PersistenceMode.SnapshotPerTransaction)
            {
                CreateSnapshot();
            }
        }

        private void EnsureAuthorized(object securable)
        {
            var principal = Thread.CurrentPrincipal;
            if (!_authorizer.Allows(securable, principal))
            {
                var msg = String.Format("Authorization failed, user {0}, transaction type: {1}", principal.Identity.Name, securable.GetType().FullName);
                throw new UnauthorizedAccessException(msg);
            }
        }

        internal byte[] GetSnapshot(IFormatter formatter = null)
        {
            var memoryStream = new MemoryStream();
            WriteSnapshotToStream(memoryStream, formatter);
            return memoryStream.GetBuffer();
        }

        /// <summary>
        /// Serialize the current model to a stream
        /// </summary>
        /// <param name="stream">A writeable stream</param>
        /// <param name="formatter">A specific formatter, otherwise the default formatter</param>
        public void WriteSnapshotToStream(Stream stream, IFormatter formatter = null)
        {
            formatter = formatter ?? _config.CreateFormatter(FormatterUsage.Snapshot);
            _kernel.Read(model => formatter.Serialize(stream, model));
        }

        /// <summary>
        /// Writes a snapshot reflecting the current state of the model to the associated <see cref="ICommandStore"/>
        /// <remarks>The snapshot is a read operation blocking writes but not other reads (unless using an ImmutablilityKernel).</remarks>
        /// </summary>
        public void CreateSnapshot()
        {
            _kernel.Read(model => _snapshotStore.WriteSnapshot(model));
        }

        private void Rollback()
        {
            //release memory held by the corrupted model
            _kernel = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            Restore();
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                //todo: bad dependency, use events instead
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
        /// Shuts down the engine and any associated open resources
        /// </summary>
        public void Close()
        {
            Dispose(true); 
        }

        private void EnsureRunning()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().FullName);
        }



        /// <summary>
        /// Load an engine from the specified location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Engine Load(string location)
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return Load(config);
        }

        /// <summary>
        /// Load an engine from a location specified by the provided EngineConfiguration
        /// </summary>
        /// <param name="config"></param>
        /// <returns>A non generic Engine</returns>
        public static Engine Load(EngineConfiguration config)
        {
            if (!config.Location.HasJournal) 
                throw new InvalidOperationException("Specify location to load from in non-generic load");
            
            var model = new ModelLoader(config).LoadModel();
            return new Engine(model, config);
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
            var model = (TModel) new ModelLoader(config).LoadModel();
            return new Engine<TModel>(model, config);
        }

        /// <summary>
        /// Create an engine at the specified location
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="location"></param>
        /// <returns>The newly created engine</returns>
        public static Engine<TModel> Create<TModel>(string location) where TModel : Model, new()
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return Create<TModel>(config);
        }

        /// <summary>
        /// Create by writing a ModelCreated entry to the journal
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Engine<TModel> Create<TModel>(EngineConfiguration config = null) where TModel : Model, new()
        {
            config = config ?? EngineConfiguration.Create();
            if (!config.Location.HasJournal) config.Location.SetLocationFromType<TModel>();
            var commandStore = config.CreateCommandStore();
            if (!commandStore.IsEmpty) throw new InvalidOperationException("Cannot Create(): empty CommandStore required");
            if (!config.CreateSnapshotStore().IsEmpty) throw new InvalidOperationException("Can't Create(): empty SnapshotStore required");
            JournalAppender.Create(0, commandStore).AppendModelCreated<TModel>();
            return Load<TModel>(config);
        }

        /// <summary>
        /// Create from an existing model by writing a snapshot
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Engine<TModel> Create<TModel>(TModel model, EngineConfiguration config = null) where TModel : Model
        {
            config = config ?? EngineConfiguration.Create();
            if (!config.Location.HasJournal) config.Location.SetLocationFromType<TModel>();
            ISnapshotStore store = config.CreateSnapshotStore();
            store.WriteSnapshot(model);
            return Load<TModel>(config);
        }

        /// <summary>
        /// Load if exists, otherwise Create and Load.
        /// </summary>
        /// <typeparam name="TModel">The type of the model</typeparam>
        /// <param name="location">The absolute or relative location</param>
        /// <returns></returns>
        public static Engine<TModel> LoadOrCreate<TModel>(string location) where TModel : Model, new()
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return LoadOrCreate<TModel>(config);
        }

        /// <summary>
        /// Load or create the specified type from the
        /// location according to EngineConfiguration.Location
        /// </summary>
        /// <typeparam name="TModel">The type of the model</typeparam>
        /// <param name="config">The configuration to use</param>
        /// <returns>A running engine</returns>
        public static Engine<TModel> LoadOrCreate<TModel>(EngineConfiguration config = null) where TModel : Model, new()
        {
            config = config ?? EngineConfiguration.Create();
            if (!config.Location.HasJournal) config.Location.SetLocationFromType<TModel>();
            Engine<TModel> result = null;

            var store = config.CreateCommandStore();

            if (store.IsEmpty)
            {
                result = Create(new TModel(), config);
                _log.Debug("Engine Created");
            }
            else
            {
                result = Load<TModel>(config);
                _log.Debug("Engine Loaded");
            }
            return result;
        }

    }
}
