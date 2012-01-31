using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading;

namespace LiveDomain.Core
{


    /// <summary>
    /// Engine is responsible for executing commands and queries against
    /// the model while conforming to ACID.
    /// </summary>
    /// <typeparam name="M"></typeparam>
	public class Engine : IDisposable
    {

        /// <summary>
        /// The prevalent system. The database. Your single aggregate root. The graph.
        /// </summary>
        protected Model _theModel;

        /// <summary>
        /// All configuration settings, cloned in the constructor
        /// </summary>
        EngineConfiguration _config;
        IStorage _storage;
        ILockStrategy _lock;
        ISerializer _serializer;
        bool _isDisposed = false;
        ICommandJournal _commandJournal;
        
 
        /// <summary>
        /// Shuts down the engine
        /// </summary>
        public void Close()
        {
            if (!_isDisposed)
            {
                if (_config.SnapshotBehavior == SnapshotBehavior.OnShutdown)
                {
                    //Allow reading while snapshot is being taken
                    //but no modifications after that
                    _lock.EnterUpgrade(); 
                    CreateSnapshotImpl("auto");

                }
                _lock.EnterWrite();
                _isDisposed = true;
                _commandJournal.Close();        
                    
            }
        }


        private void Restore<M>(Func<M> constructor) where M : Model
        {

            JournalSegmentInfo segment;

            _theModel = _storage.GetMostRecentSnapshot(out segment);

            if (_theModel == null) 
            {
                if(constructor == null)  throw new ApplicationException("No initial snapshot");
                _theModel = constructor.Invoke();
            }
            
            _theModel.SnapshotRestored();
            foreach (var command in _commandJournal.GetEntriesFrom(segment).Select(entry => entry.Item))
            {
                command.Redo(_theModel);
            }
            _theModel.JournalRestored();
        }


        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().FullName);
        }


        protected Engine(Func<Model> constructor, EngineConfiguration config)
        {
            _serializer = config.CreateSerializer();
            
            //Prevent modification from outside
            _config = _serializer.Clone(config);

            _storage = _config.CreateStorage();
            _lock = _config.CreateLockingStrategy();

            _commandJournal = _config.CreateCommandJournal(_storage);
            Restore(constructor);
            _commandJournal.Open();
            
            if (_config.SnapshotBehavior == SnapshotBehavior.AfterRestore)
            {
                Log.Write("Starting snaphot job on threadpool");
                
                ThreadPool.QueueUserWorkItem((o) => CreateSnapshot("auto"));

                //Give the thread a chance to start and aquire the readlock
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }
        }


        internal byte[] GetSnapshot()
        {
            try
            {
                _lock.EnterRead();
                return _serializer.Serialize(_theModel);

            }
            finally
            {
                _lock.Exit();
            }
        }

        #region Execute overloads
        
        public object Execute(Query query)
        {
        	return Execute<Model, object>(M => query.ExecuteStub(M));
        }

		public T Execute<M, T>(Func<M, T> query) where M : Model
        {
            ThrowIfDisposed();
            try
            {
                _lock.EnterRead();
                T result = query.Invoke(_theModel as M);
                //TODO: Possible comparison of value type to null
                if (_config.CloneResults && result != null) result = _serializer.Clone(result);
                return result;
            }
            catch (TimeoutException)
            {
                ThrowIfDisposed();
                throw;
            }
            finally
            {
                _lock.Exit();
            }

        }

        public object Execute(Command command)
        {
            ThrowIfDisposed();

            Command commandToSerialize = command;
            if (_config.CloneCommands) command = _serializer.Clone(command);
            
            try
            {
                _lock.EnterUpgrade();
                command.PrepareStub(_theModel);
                _lock.EnterWrite();
                object result = command.ExecuteStub(_theModel);
                //TODO: We might benefit from downgrading the lock at this point
                //TODO: We could run the 2 following statements in parallel
                if (_config.CloneResults && result != null) result = _serializer.Clone(result);
                _commandJournal.Append(commandToSerialize);
                return result;
            }
            catch (TimeoutException)
            {
                ThrowIfDisposed();
                throw; 
            }
            catch (CommandFailedException) { throw; }
            catch (Exception ex) 
            {
                Restore(() => (Model)Activator.CreateInstance(_theModel.GetType())); //TODO: Or shutdown based on setting
                throw new CommandFailedException("Command threw an exception, state was rolled back, see inner exception for details", ex);
            }
            finally
            {
                _lock.Exit();
            }
        }

        #endregion

        #region Snapshot methods
        public void CreateSnapshot()
        {
            CreateSnapshot(String.Empty);
        }

        public void CreateSnapshot(string name)
        {
            try
            {
                _lock.EnterRead();
                CreateSnapshotImpl(name);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _lock.Exit();
            }
        }

        private void CreateSnapshotImpl(string name)
        {
            Log.Write("BeginSnapshot:" + name);
            _storage.WriteSnapshot(_theModel, name);
            _commandJournal.CreateNextSegment();
            Log.Write("EndSnapshot:" + name);
        }

        #endregion

        public void Dispose()
        {
            this.Close();
        }


        #region Static non-generic Load and Create methods

        public static Engine Load(string location)
        {
            return Load(new EngineConfiguration(location));
        }

        public static Engine Load(EngineConfiguration config)
        {
            if (!config.HasLocation) throw new InvalidOperationException("Specify location to load from in non-generic load");
            config.CreateStorage().VerifyCanLoad();
            var engine = new Engine(null, config);
            return engine;
        }

        public static Engine Create(Model model, string location)
        {
            return Create(model, new EngineConfiguration(location));
        }

        public static Engine Create(Model model, EngineConfiguration config)
        {
            if (!config.HasLocation) config.SetDefaultLocation(model.GetType());
            return Create<Model>(model, config);

        }
        

        #endregion
        
        #region Static generic Load methods

        /// <summary>
        /// Load using default configuration and location
        /// </summary>
        /// <returns>A strongly typed generic Engine</returns>
        public static Engine<M> Load<M>() where M : Model
        {
            return Load<M>(new EngineConfiguration());
        }

        /// <summary>
        /// Load from location using the default EngineConfiguration
        /// </summary>
        /// <typeparam name="M"></typeparam>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Engine<M> Load<M>(string location) where M : Model
    	{
    		return Load<M>(new EngineConfiguration(location));
    	}

        /// <summary>
        /// Load using an explicit configuration.
        /// </summary>
        /// <typeparam name="M"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
    	public static Engine<M> Load<M>(EngineConfiguration config) where M : Model
    	{
            if (!config.HasLocation) config.SetDefaultLocation<M>();
            config.CreateStorage().VerifyCanLoad();
			var engine = new Engine<M>(config);
    		return engine;
    	}
        #endregion

        #region Generic Create methods

        /// <summary>
        /// Create using default constructor, default EngineConfiguration at the default location
        /// </summary>
        /// <typeparam name="M"></typeparam>
        /// <returns></returns>
        public static Engine<M> Create<M>() where M : Model
        {
            return Create<M>(new EngineConfiguration());
        }

        public static Engine<M> Create<M>(string location) where M : Model
        {
            return Create<M>(new EngineConfiguration(location));
        }

        public static Engine<M> Create<M>(M model, string location) where M : Model
        {
            return Create<M>(model, new EngineConfiguration(location));
        }

        public static Engine<M> Create<M>(EngineConfiguration config) where M : Model
        {
            M model = Activator.CreateInstance<M>();
            return Create(model, config);
        }

        public static Engine<M> Create<M>(M model, EngineConfiguration config) where M : Model
        {
            if (!config.HasLocation) config.SetDefaultLocation<M>();
            IStorage storage = config.CreateStorage();
            storage.Create(model);
            return Load<M>(config);
        }

        #endregion

        #region Static generic LoadOrCreate methods


        public static Engine<M> LoadOrCreate<M>() where M : Model, new()
        {
            return LoadOrCreate<M>(new EngineConfiguration());
        }


        public static Engine<M> LoadOrCreate<M>(string location) where M : Model, new()
        {
            EngineConfiguration config = new EngineConfiguration(location);
            return LoadOrCreate<M>(config);
        }

        public static Engine<M> LoadOrCreate<M>(EngineConfiguration config) where M : Model, new()
        {

            Func<M> constructor = () => Activator.CreateInstance<M>();
            return LoadOrCreate<M>(constructor, config);
        }

        public static Engine<M> LoadOrCreate<M>(Func<M> constructor) where M : Model
        {
            return LoadOrCreate(constructor, new EngineConfiguration());
        }

        public static Engine<M> LoadOrCreate<M>(Func<M> constructor, EngineConfiguration config) where M : Model
        {
            if (constructor == null) throw new ArgumentNullException("constructor");
            if(config == null) throw new ArgumentNullException("config");
            if (!config.HasLocation) config.SetDefaultLocation<M>();
            Engine<M> result = null;

            var storage = config.CreateStorage();

            if (storage.Exists)
            {
                result = Load<M>(config);
                Log.Write("Engine Loaded");
            }
            else if (storage.CanCreate)
            {
                result = Create<M>(constructor.Invoke(), config);
                Log.Write("Engine Created");
            }
            else throw new ApplicationException("Couldn't load or create");
            return result;
        }

        #endregion
    }
}
