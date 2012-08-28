using System;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LiveDomain.Core.Logging;
using LiveDomain.Core.Security;

namespace LiveDomain.Core.Configuration
{

    [Serializable]
    public class EngineConfiguration
    {

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        public const string DefaultDateFormatString =  "yyyy.MM.dd.hh.mm.ss.fff";
        public const long DefaultJournalSegmentSizeInBytes = 1024 * 1024;


        string _location, _snapshotLocation;

        /// <summary>
        /// The location of the command journal and snapshots. A directory path when using FileStorage, 
        /// a connection string when using SqlStorage.
        /// Assigning a relative path will resolve to current directory or App_Data if running in a web context
        /// </summary>
        public string Location
        {
            get { return Path.Combine(GetDefaultDirectory(), _location); }
            set { _location = value; }
        }
        



        /// <summary>
        /// Same as TargetLocation unless set to some other location
        /// </summary>
        public string SnapshotLocation
        {
            get
            {
                return _snapshotLocation ?? Location;
            }
            set
            {
                if (value == null || value == Location) _snapshotLocation = null;
                else _snapshotLocation = value;
            }
        }

        

        /// <summary>
        /// True if the snapshotlocation differs from the location of the journal
        /// </summary>
        public bool HasAlternativeSnapshotLocation
        {
            get
            {
                return _snapshotLocation != null;
            }

        }

        /// <summary>
        /// When segment exceeds this size a new segment is created
        /// </summary>
        public long JournalSegmentSizeInBytes { get; set; }


        /// <summary>
        /// Make a deep copy of all command and query results so no references are passed out from the model. Default is true.
        /// <remarks>
        /// Set to false if you are certain that results will not be modified by client code. Note also that 
        /// the state of the resultset can be modified by a subsequent command rendering the result graph inconsistent.</remarks>
        /// </summary>
        /// 
        public bool CloneResults { get; set; }

        /// <summary>
        /// Make a deep copy of each command prior to execution. This will force a fast 
        /// failure of commands that wont serialize.
        /// </summary>
        public bool CloneCommands { get; set; }


        /// <summary>
        /// Maximum time to wait for any read or write lock
        /// </summary>
        public TimeSpan LockTimeout { get; set; }


        public SnapshotBehavior SnapshotBehavior{get;set;}

        public CompressionMethod Compression 
        { 
            get { return CompressionMethod.None; } 
            set 
            {
                if (value != CompressionMethod.None)
                {
                    throw new ArgumentException("Compression not yet supported", "CompressionMethod");
                }
            } 
        }

        public StorageMode StorageMode { get; set; }
        public ConcurrencyMode Concurrency{ get; set; }
        public SerializationMethod SerializationMethod { get; set; }
        public JournalWriterPerformanceMode JournalWriterPerformance { get; set; }
        public string DateFormatString { get; set; }


        public EngineConfiguration() : this(null)
        {

        }

        public EngineConfiguration(string targetLocation)
        {

            Location = targetLocation;

            //Set default values
            LockTimeout = DefaultTimeout;
            Compression = CompressionMethod.None;
            Concurrency = ConcurrencyMode.MultipleReadersOrSingleWriter;
            SerializationMethod = SerializationMethod.NetBinaryFormatter;
            JournalWriterPerformance = JournalWriterPerformanceMode.Synchronous;
            JournalSegmentSizeInBytes = DefaultJournalSegmentSizeInBytes;
            DateFormatString = DefaultDateFormatString;
            CloneResults = true;
            CloneCommands = true;
        }


        #region FactoryMethods

        public virtual string KeyTemplate
        {
            get { return "LiveDbConfiguration.{0}"; }
        }

        private static EngineConfiguration _current = Create();

        public static EngineConfiguration Current
        {
            get { return _current; }
            set { _current = value; }
        }

        public static EngineConfiguration Create()
        {
            //we need an instance to be able to call an instance method
            var bootStrapper = new EngineConfiguration();

            //look for specific implementation in config file, otherwise return self
            return bootStrapper.LoadFromConfigOrDefault(() => bootStrapper);
        }


        protected virtual T LoadFromConfigOrDefault<T>(Func<T> @default = null)
        {
            @default = @default ?? (() => (T)Activator.CreateInstance(typeof(T)));
            string configKey = ConfigKeyFromType(typeof(T));
            var configTypeName = ConfigurationManager.AppSettings[configKey];
            if (!String.IsNullOrEmpty(configTypeName))
            {
                return InstanceFromTypeName<T>(configTypeName);
            }
            else
            {
                return @default.Invoke();
            }
        }

        protected virtual T InstanceFromTypeName<T>(string typeName)
        {
            try
            {
                Type type = Type.GetType(typeName);
                return (T)Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                String messageTemplate = "Can't load type {0}, see inner exception for details";
                throw new ConfigurationErrorsException(String.Format(messageTemplate, typeName), exception);
            }
        }


        private ILogFactory _logFactory = null;

        protected internal virtual ILogFactory GetLogFactory()
        {
            if (_logFactory == null)
            {
                _logFactory = LoadFromConfigOrDefault<ILogFactory>(() => new InternalLogFactory());
            }
            return _logFactory;
        }

        public virtual void SetLogFactory(ILogFactory logFactory)
        {
            _logFactory = logFactory;
        }

        protected internal virtual IAuthorizer<Type> GetAuthorizer()
        {
            return new PermissionSet<Type>(Permission.Allowed);
        }

        public virtual EngineConfiguration GetEngineConfiguration()
        {
            return LoadFromConfigOrDefault<EngineConfiguration>();
        }

        protected internal virtual IStorage GetCustomStorage(EngineConfiguration engineConfiguration)
        {
            return LoadFromConfig<IStorage>();
        }


        protected virtual T LoadFromConfig<T>()
        {
            string configKey = String.Format(KeyTemplate, ConfigKeyFromType(typeof(T)));
            var configTypeName = ConfigurationManager.AppSettings[configKey];
            return InstanceFromTypeName<T>(configTypeName);
        }

        protected virtual String ConfigKeyFromType(Type type)
        {
            return type.Name;
        }
        internal ISerializer CreateSerializer()
        {
            return new Serializer(CreateFormatter());
        }

        /// <summary>
        /// Factory method choosing concrete type based on SerializationMethod property
        /// </summary>
        /// <returns></returns>

        internal IFormatter CreateFormatter()
        {
            switch (SerializationMethod)
            {
                case SerializationMethod.NetBinaryFormatter:
                    return new BinaryFormatter();
                default:
                    throw new Exception("Missing case for switch on SerializationMethod");
            }
        }

        /// <summary>
        /// Factory method choosing concrete type based on ConcurrencyMode property
        /// </summary>
        /// <returns></returns>
        internal ILockStrategy CreateLockingStrategy()
        {
            switch (Concurrency)
            {
                case ConcurrencyMode.SingleReaderOrWriter:
                    return new SingleThreadedLockingStrategy(LockTimeout);
                
                case ConcurrencyMode.MultipleReadersOrSingleWriter:
                    return new ReaderWriterLockSlimStrategy(LockTimeout);
                
                case ConcurrencyMode.MultipleReadersAndWriters:
                    return new NullLockingStrategy();
                
                default:
                    throw new Exception("Missing case for switch on ConcurrencyMode");
            }
        }

        virtual internal IJournalWriter CreateJournalWriter(Stream stream)
        {

            ISerializer serializer = CreateSerializer();

            switch (JournalWriterPerformance)
            {
                case JournalWriterPerformanceMode.Synchronous:
                    return new SynchronousJournalWriter(stream,serializer);
                case JournalWriterPerformanceMode.Asynchronous:
                    return new AsynchronousJournalWriter(new SynchronousJournalWriter(stream,serializer));
                default:
                    throw new Exception("Missing case for switch on JournalWriterPerformanceMode");
            }
        }

        virtual internal IStorage CreateStorage()
        {
            switch (this.StorageMode)
            {
                case StorageMode.None: 
                    return new NullStorage();
                case StorageMode.FileSystem:
                    return new FileStorage(this);
                case StorageMode.Custom:
                    return GetCustomStorage(this);
                default:
                    throw new Exception("Unsupported storage mode: " + this.StorageMode.ToString());
            }
        }


        virtual internal ICommandJournal CreateCommandJournal(IStorage _storage)
        {
            return new CommandJournal(this, _storage);
        }

        virtual internal IAuthorizer<Type> CreateAuthorizer()
        {
            return GetAuthorizer();
        }

        #endregion


        /// <summary>
        /// The default directory to use if the Location is relative
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultDirectory()
        {

            string result = Directory.GetCurrentDirectory();

            //Attempt web
            try
            {
                string typeName = "System.Web.HttpContext, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
                Type type = Type.GetType(typeName);
                object httpContext = type.GetProperty("Current").GetGetMethod().Invoke(null, null);
                object httpServer = type.GetProperty("Server").GetGetMethod().Invoke(httpContext, null);
                result = (string)httpServer.GetType().GetMethod("MapPath").Invoke(httpServer, new object[] { "~/App_Data" });
            }
            catch { }
            return result;
        }

        public bool HasLocation 
        { 
            get 
            {
                return !String.IsNullOrEmpty(_location);
            }
        }

        internal void SetLocationFromType<M>()
        {
            Location = typeof (M).Name;
        }
    }

}
