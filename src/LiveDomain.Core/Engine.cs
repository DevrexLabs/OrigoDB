using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

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

        /// <summary>
        /// 
        /// </summary>
        IPersistentStorage _storage;


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
                try
                {
                    _lock.EnterWrite();
                    _commandJournal.Close();        
                    _isDisposed = true;

                }
                finally
                {
                    _lock.Exit();
                }
            }
        }


        private void Restore()
        {

            JournalFragmentInfo fragment;

            _theModel = _storage.GetMostRecentSnapshot(out fragment);
            if (_theModel == null) throw new ApplicationException("No initial snapshot");
            
            foreach (var command in _commandJournal.GetEntriesFrom(fragment).Select(entry => entry.Item))
            {
                command.Redo(_theModel);
            }
        }


        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().FullName);
        }


        protected Engine(EngineConfiguration config)
        {
            _serializer = config.CreateSerializer();
            
            //Prevent modification from outside
            _config = _serializer.Clone(config);

            _storage = _config.CreateStorage();
            _lock = _config.CreateLockingStrategy();

            _commandJournal = _config.CreateCommandJournal(_storage);
            Restore();
            _theModel.OnRestore();
            _commandJournal.Open();
        }


        #region Execute overloads
        
        public object Execute(Query query)
        {
        	return Execute<Model, object>(M => query.ExecuteStub(M));
        }

		public T Execute<M, T>(Func<M, T> query) where M : Model
        {
            ThrowIfDisposed();
            _lock.EnterRead();

            try
            {
                T result = query.Invoke(_theModel as M);
                if (_config.CloneResults) result = _serializer.Clone(result);
                return result;
            }
            finally
            {
                _lock.Exit();
            }

        }

        public object Execute(Command command)
        {
            ThrowIfDisposed();
            if(_config.CloneCommands) command = _serializer.Clone(command);
            _lock.EnterUpgrade();
            try
            {
                command.PrepareStub(_theModel);
                _lock.EnterWrite();
                object result = command.ExecuteStub(_theModel);
                _commandJournal.Append(command);
                if (_config.CloneResults) result = _serializer.Clone(result);
                return result;
            }
            catch (TimeoutException) { throw; }
            catch (CommandFailedException) { throw; }
            catch (FatalException) { Close(); throw; }
            catch (Exception ex) 
            {
                Restore(); //TODO: Or shutdown based on setting
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
                _storage.WriteSnapshot(_theModel, name);
                _commandJournal.Rollover();
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

        #endregion

        public void Dispose()
        {
            this.Close();
        }


        #region Static non-generic Load and Create methods

        public static Engine Load(string targetLocation)
        {
            return Load(new EngineConfiguration(targetLocation));
        }

        public static Engine Load(EngineConfiguration settings)
        {
            settings.CreateStorage().VerifyCanLoad();
            var engine = new Engine(settings);
            return engine;
        }

        public static Engine Create(Model model, string targetLocation)
        {
            return Create(model, new EngineConfiguration(targetLocation));
        }

        public static Engine Create(Model model, EngineConfiguration config)
        {
            return Create<Model>(model, config);

        }
        

        #endregion
        
        #region Static generic Load methods

        /// <summary>
        /// Load using default configuration. Location defaults to either 
        /// ~/App_Data\typeof(M).Name or CurrentDirectory()\typeof(M).Name
        /// </summary>
        /// <returns>A strongly typed generic Engine</returns>
        public static Engine<M> Load<M>() where M : Model
        {
            string current = Directory.GetCurrentDirectory();
            string targetDirectory = Path.Combine(current, typeof(M).Name);
            return Load<M>(targetDirectory);
        }

        /// <summary>
        /// Load from targetDirectory using the default EngineConfiguration
        /// </summary>
        /// <typeparam name="M"></typeparam>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Engine<M> Load<M>(string location) where M : Model
    	{
    		return Load<M>(new EngineConfiguration(location));
    	}

        /// <summary>
        /// Load using a specific configuration. Path is part of the configuration
        /// </summary>
        /// <typeparam name="M"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
    	public static Engine<M> Load<M>(EngineConfiguration config) where M : Model
    	{
			var engine = new Engine<M>(config);
			engine.Restore();
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
            string location = GetDefaultLocation() + @"\" + typeof(M).Name;
            return Create<M>(location);
        }


        private static string GetDefaultLocation()
        {
            
            string result = Directory.GetCurrentDirectory();
            
            //Attempt web
            try
            {
                string typeName = "System.Web.HttpContext, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
                Type type = Type.GetType(typeName);
                object httpContext = type.GetProperty("Current").GetGetMethod().Invoke(null, null);
                object httpRequest = type.GetProperty("Request").GetGetMethod().Invoke(httpContext,null);
                result = (string) httpRequest.GetType().GetProperty("ApplicationPath").GetGetMethod().Invoke(httpRequest,null);
                result = result.TrimEnd('\\') + "App_Data";
            }
            catch{}
            return result;
        }

        public static Engine<M> Create<M>(string location) where M : Model
        {
            return Create<M>(new EngineConfiguration(location));
        }

        public static Engine<M> Create<M>(EngineConfiguration config) where M : Model
        {
            M model = Activator.CreateInstance<M>();
            return Create(model, config);
        }

        public static Engine<M> Create<M>(M model, EngineConfiguration config) where M : Model
        {
            IPersistentStorage storage = config.CreateStorage();
            storage.Create(model);
            return Load<M>(config);
        }

        #endregion

        #region Static generic CreateOrLoad methods


        public static Engine<M> LoadOrCreate<M>() where M : Model, new()
        {

            //use current directory as default. TODO: 
            string location = GetDefaultLocation() + @"\" + typeof(M).Name;

            return LoadOrCreate<M>(location);
        }


        public static Engine<M> LoadOrCreate<M>(string location) where M : Model, new()
        {
            EngineConfiguration config = new EngineConfiguration(location);
            return LoadOrCreate<M>(config);
        }

        public static Engine<M> LoadOrCreate<M>(EngineConfiguration config) where M : Model, new()
        {

            Func<M> constructor = () => (M) Activator.CreateInstance<M>();
            return LoadOrCreate<M>(constructor, config);
        }

        public static Engine<M> LoadOrCreate<M>(Func<M> constructor, EngineConfiguration config) where M : Model
        {
            if (constructor == null) throw new ArgumentNullException("constructor");
            if(config == null) throw new ArgumentNullException("config");
            Engine<M> result = null;
            try
            {
                result = Load<M>(config);
                Log.Write("Engine Loaded");
            }
            catch (Exception)
            {
                result = Create(constructor.Invoke(), config);
                Log.Write("Engine Created");
            }
            return result;
        }

        #endregion
    }
}
