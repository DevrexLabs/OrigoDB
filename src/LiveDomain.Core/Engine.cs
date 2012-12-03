using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading;
using LiveDomain.Core.Configuration;
using LiveDomain.Core.Logging;
using LiveDomain.Core.Proxy;
using LiveDomain.Core.Security;
using System.Collections;
using LiveDomain.Core.Utilities;

namespace LiveDomain.Core
{


    /// <summary>
    /// Engine is responsible for executing commands and queries against
    /// the model while conforming to ACID.
    /// </summary>
    public partial class Engine : IDisposable
    {
        EngineConfiguration _config;
        bool _isDisposed = false;
        static ILog _log = LogProvider.Factory.GetLogForCallingType();
        IAuthorizer<Type> _authorizer;

        private readonly Kernel _kernel;

        public EngineConfiguration Config { get { return _config; } }

        /// <summary>
        /// Shuts down the engine
        /// </summary>
        public void Close()
        {
            if (!_isDisposed)
            {
                Core.Config.Engines.Remove(this);
                if (_config.SnapshotBehavior == SnapshotBehavior.OnShutdown) CreateSnapshot();
                _isDisposed = true;
                _kernel.Dispose();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().FullName);
        }


        protected Engine(Func<Model> constructor, EngineConfiguration config)
        {
            _config = config;

            var store = _config.CreateStore();
            store.Load();

            _kernel = _config.CreateKernel(store);
            _kernel.Restore(constructor);
            
            _authorizer = _config.CreateAuthorizer();

            if (_config.SnapshotBehavior == SnapshotBehavior.AfterRestore)
            {
                _log.Info("Starting snaphot job on threadpool");

                ThreadPool.QueueUserWorkItem((o) => CreateSnapshot());

                //Give the snapshot thread a chance to start and aquire the readlock
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }

            Core.Config.Engines.AddEngine(config.Location.OfJournal, this);
        }

        public object Execute(Query query)
        {
            return Execute<Model, object>(model => query.ExecuteStub(model));
        }

        public T Execute<M, T>(Func<M, T> lambdaQuery) where M : Model
        {
            ThrowIfDisposed();
            ThrowUnlessAuthenticated(lambdaQuery.GetType());
            return (T)ExecuteQuery(new DelegateQuery<M, T>(lambdaQuery));
        }

        public T Execute<M, T>(Query<M, T> query) where M : Model
        {
            ThrowIfDisposed();
            ThrowUnlessAuthenticated(query.GetType());
            return ExecuteQuery(query);
        }

        private T ExecuteQuery<M, T>(Query<M, T> query) where M : Model
        {
            return _kernel.ExecuteQuery(query);
        }

        public object Execute(Command command)
        {
            ThrowIfDisposed();
            ThrowUnlessAuthenticated(command.GetType());
            return _kernel.ExecuteCommand(command);
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
            //TODO: Verify GetBuffer!
            MemoryStream memoryStream = new MemoryStream();
            WriteSnapshotToStream(memoryStream);
            return memoryStream.GetBuffer();
        }

        internal void WriteSnapshotToStream(Stream stream)
        {
            var serializer = _config.CreateSerializer();
            _kernel.Read(m =>serializer.Write(m,stream));
        }
 
        public void CreateSnapshot()
        {
            _kernel.CreateSnapshot();
        }


        public void Dispose()
        {
            Close();
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
	        Engine engine;
			if(!Core.Config.Engines.TryGetEngine(config.Location.OfJournal,out engine))
			{
				config.CreateStore().VerifyCanLoad();
				engine = new Engine(null, config);
			}
            return engine;
        }

        public static Engine Create(Model model, string location = null)
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
        /// <typeparam name="M"></typeparam>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Engine<M> Load<M>(string location) where M : Model
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return Load<M>(config);
        }

        /// <summary>
        /// Load using an explicit configuration.
        /// </summary>
        /// <typeparam name="M"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Engine<M> Load<M>(EngineConfiguration config = null) where M : Model
        {
            config = config ?? EngineConfiguration.Create();
            if (!config.Location.HasJournal) config.Location.SetLocationFromType<M>();
	        Engine engine;
			if (!Core.Config.Engines.TryGetEngine(config.Location.OfJournal, out engine))
			{
				config.CreateStore().VerifyCanLoad();
				engine = new Engine<M>(config);
			}           
            return (Engine<M>) engine;
        }
        #endregion

        #region Generic Create methods

        public static Engine<M> Create<M>(string location) where M : Model
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return Create<M>(config);
        }

        public static Engine<M> Create<M>(M model, string location = null) where M : Model
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return Create<M>(model, config);
        }

        public static Engine<M> Create<M>(EngineConfiguration config = null) where M : Model
        {
            config = config ?? EngineConfiguration.Create();
            M model = Activator.CreateInstance<M>();
            return Create(model, config);
        }

        public static Engine<M> Create<M>(M model, EngineConfiguration config) where M : Model
        {
            if (!config.Location.HasJournal) config.Location.SetLocationFromType<M>();
            IStore store = config.CreateStore();
            store.Create(model);
            return Load<M>(config);
        }

        #endregion

        #region Static generic LoadOrCreate methods


        public static Engine<M> LoadOrCreate<M>(string location = null) where M : Model, new()
        {
            var config = EngineConfiguration.Create();
            config.Location.OfJournal = location;
            return LoadOrCreate<M>(config);
        }

        public static Engine<M> LoadOrCreate<M>(EngineConfiguration config) where M : Model, new()
        {

            config = config ?? EngineConfiguration.Create();
            Func<M> constructor = () => Activator.CreateInstance<M>();
            return LoadOrCreate<M>(constructor, config);
        }

        public static Engine<M> LoadOrCreate<M>(Func<M> constructor, EngineConfiguration config = null) where M : Model
        {
            config = config ?? EngineConfiguration.Create();
            if (constructor == null) throw new ArgumentNullException("constructor");
            if (config == null) throw new ArgumentNullException("config");
            if (!config.Location.HasJournal) config.Location.SetLocationFromType<M>();
            Engine<M> result = null;

            var store = config.CreateStore();

            if (store.Exists)
            {
                result = Load<M>(config);
                _log.Debug("Engine Loaded");
            }
            else
            {
                result = Create<M>(constructor.Invoke(), config);
                _log.Debug("Engine Created");
            }
            return result;
        }

        #endregion
    }
}
