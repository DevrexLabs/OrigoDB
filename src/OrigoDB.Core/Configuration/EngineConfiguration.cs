using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using OrigoDB.Core.Configuration;
using OrigoDB.Core.Security;
using OrigoDB.Core.Storage.Sql;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{
    public class EngineConfiguration
    {
        protected TeenyIoc Registry;

        private CustomBinder _binder;

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        public const string DefaultDateFormatString = "yyyy.MM.dd.hh.mm.ss.fff";
        public const int DefaultMaxBytesPerJournalSegment = 1024 * 1024 * 8;
        public const int DefaultMaxCommandsPerJournalSegment = 10000;


        public SqlSettings SqlSettings { get; set; }
        
        public IsolationSettings Isolation { get; set; }

        /// <summary>
        /// Isolated types will not be cloned when CloneStrategy is Auto.
        /// Add types known to guarantee isolation.
        /// </summary>
        public ISet<Type> IsolatedTypes { get; set; }

        /// <summary>
        /// Append journal entries using a background thread
        /// sacrificing durability under certain circumstances
        /// for performance.
        /// </summary>
        public bool AsynchronousJournaling { get; set; }

        /// <summary>
        /// Set's the type of persistence used
        /// </summary>
        public PersistenceMode PersistenceMode { get; set; }

        /// <summary>
        /// Choose type of kernel
        /// </summary>
        public Kernels Kernel { get; set; }

        /// <summary>
        /// If assigned, write <see cref="Packet"/>s to the journal using the options specified.
        /// Otherwise serialize object graphs directly to the underlying stream
        /// </summary>
        public PacketOptions? PacketOptions { get; set; }

        /// <summary>
        /// Maximum time to wait for any read or write lock
        /// </summary>
        public TimeSpan LockTimeout { get; set; }


        /// <summary>
        /// When to take automatic snapshots
        /// </summary>
        public SnapshotBehavior SnapshotBehavior { get; set; }

        /// <summary>
        /// Effects which ISynchronizer is chosen by CreateSynchronizer()
        /// </summary>
        public SynchronizationMode Synchronization { get; set; }

        /// <summary>
        /// The type of storage to use for the command journal. File is default.
        /// </summary>
        public StorageType JournalStorage { get; set; }

        /// <summary>
        /// Maximum number of journal entries per segment. Applies only to storage 
        /// providers which split up the journal in segments and ignored by others.
        /// </summary>
        public int MaxEntriesPerJournalSegment { get; set; }

        /// <summary>
        /// Maximum number of bytes entries per segment. Applies only to storage 
        /// providers which split up the journal in segments and ignored by others.
        /// </summary>
        public int MaxBytesPerJournalSegment { get; set; }

        /// <summary>
        /// Path where journal files will be saved. Relative current directory or App_Data in web context.
        /// Leave unassigned (null) and type name of model will be used.
        /// </summary>
        public String JournalPath { get; set; }

        /// <summary>
        /// Directory where snapshots are written. Leave unassigned and JournalPath will be used.
        /// </summary>
        public string SnapshotPath { get; set; }

        /// <summary>
        /// Asynchronously delete journal files/entries after a successfully written snapshot
        /// </summary>
        public bool TruncateJournalOnSnapshot { get; set; }

        /// <summary>
        /// Create an EngineConfiguration instance using default values
        /// </summary>
        /// <param name="targetLocation"></param>
        public EngineConfiguration(string targetLocation = null)
        {
            JournalPath = targetLocation;

            //Set default values
            Kernel = Kernels.Optimistic;
            JournalStorage = StorageType.File;
            LockTimeout = DefaultTimeout;
            Synchronization = SynchronizationMode.ReadWrite;
            AsynchronousJournaling = false;
            MaxBytesPerJournalSegment = DefaultMaxBytesPerJournalSegment;
            MaxEntriesPerJournalSegment = DefaultMaxCommandsPerJournalSegment;
            PacketOptions = null;
            PersistenceMode = PersistenceMode.Journaling;
            SqlSettings = new SqlSettings();
            Isolation = new IsolationSettings();
            IsolatedTypes = new HashSet<Type>();

            Registry = new TeenyIoc();
            Register<IAuthorizer>(c => new Authorizer(Permission.Allowed));
            Register<IFormatter>(c => new BinaryFormatter(), FormatterUsage.Default.ToString());
            InitSynchronizers();
            InitStoreTypes();
            InitKernels();
        }


        private void InitKernels()
        {
            Register<Kernel>((cfg, args) => new OptimisticKernel(cfg, (Model) args["model"]), Kernels.Optimistic.ToString());
            Register<Kernel>((cfg, args) => new RoyalFoodTaster(cfg, (Model) args["model"]), Kernels.RoyalFoodTaster.ToString());
            Register<Kernel>((cfg, args) => new ImmutabilityKernel(cfg, (Model) args["model"]), Kernels.Immutability.ToString()); 

        }

        /// <summary>
        /// Created a named registration for each SynchronizationMode enumeration value
        /// </summary>
        private void InitSynchronizers()
        {
            Register<ISynchronizer>(c => new ReadWriteSynchronizer(LockTimeout),
                                              SynchronizationMode.ReadWrite.ToString());
            Register<ISynchronizer>(c => new NullSynchronizer(),
                                              SynchronizationMode.None.ToString());
            Register<ISynchronizer>(c => new ExclusiveSynchronizer(LockTimeout),
                                              SynchronizationMode.Exclusive.ToString());
        }

        private void InitStoreTypes()
        {
            Register<ICommandStore>(cfg => new FileCommandStore(cfg), StorageType.File.ToString());
            Register<ICommandStore>(cfg => new SqlCommandStore(cfg), StorageType.Sql.ToString());
            Register<ISnapshotStore>(cfg => new FileSnapshotStore(cfg));
        }

        /// <summary>
        /// Return an IFormatter by invoking the factory function associated
        /// with the given FormatterUsage or FormatterUsage.Default if not registered.
        /// </summary>
        /// <param name="formatterUsage">The specific formatter</param>
        /// <returns>An IFormatter instance provided by the </returns>
        public virtual IFormatter CreateFormatter(FormatterUsage formatterUsage)
        {
            var formatter = Registry.CanResolve<IFormatter>(formatterUsage.ToString())
                ? Registry.Resolve<IFormatter>(formatterUsage.ToString())
                : Registry.Resolve<IFormatter>(FormatterUsage.Default.ToString());

            if (_binder != null && formatter is BinaryFormatter)
            {
                ((BinaryFormatter)formatter).Binder = _binder;
            }
    
            if (formatterUsage == FormatterUsage.Journal && PacketOptions != null)
            {
                formatter = new PacketingFormatter(formatter, PacketOptions.Value);
            }
            return formatter;
        }

        /// <summary>
        /// Gets a synchronizer based on the SynchronizationMode property
        /// </summary>
        /// <returns></returns>
        public virtual ISynchronizer CreateSynchronizer()
        {
            string registrationName = Synchronization.ToString();
            return Registry.Resolve<ISynchronizer>(registrationName);
        }

        public virtual IAuthorizer CreateAuthorizer()
        {
            return Registry.Resolve<IAuthorizer>();
        }


        public virtual ISnapshotStore CreateSnapshotStore()
        {
            var store = Registry.Resolve<ISnapshotStore>();
            store.Initialize();
            return store;
        }

        public virtual ICommandStore CreateCommandStore()
        {
            var store =  Registry.Resolve<ICommandStore>(JournalStorage.ToString());
            store.Initialize();
            return store;
        }


        protected void Register<T>(Func<EngineConfiguration, T> factory, string registrationName = "") where T : class
        {
            Registry.Register(args => factory.Invoke(this), registrationName);
        }

        protected void Register<T>(Func<EngineConfiguration, TeenyIoc.Args, T> factory,
            string registrationName = "") where T : class
        {
            Registry.Register(args => factory.Invoke(this, args), registrationName);
        }


        public void SetSynchronizerFactory(Func<EngineConfiguration, ISynchronizer> factory)
        {
            Synchronization = SynchronizationMode.Custom;
            string registrationName = Synchronization.ToString();
            Registry.Register(args => factory.Invoke(this), registrationName);
        }

        public void SetAuthorizerFactory(Func<EngineConfiguration, IAuthorizer> factory)
        {
            Register(args => factory.Invoke(this));
        }

        /// <summary>
        /// Set types to serialize to given full name of types in the serialization stream
        /// </summary>
        /// <param name="typesByName"></param>
        public void SetSerializationTypeMappings(IDictionary<string, Type> typesByName)
        {
            Ensure.NotNull(typesByName, "typesByName");
            _binder = new CustomBinder(typesByName);
        }

        /// <summary>
        /// Inject a custom IFormatter factory.
        /// </summary>
        /// <param name="factory">Function that provides an IFormatter</param>
        /// <param name="formatterUsage">What type of objects to format</param>
        public void SetFormatterFactory(Func<EngineConfiguration, FormatterUsage, IFormatter> factory, FormatterUsage formatterUsage = FormatterUsage.Default)
        {
            Register(args => factory.Invoke(this, formatterUsage), formatterUsage.ToString());
        }

        /// <summary>
        /// Inject a custom command storage factory here. StorageMode property will be set to Custom
        /// </summary>
        /// <param name="factory"></param>
        public void SetCommandStoreFactory(Func<EngineConfiguration, ICommandStore> factory)
        {
            JournalStorage = StorageType.Custom;
            Register(args => factory.Invoke(this), JournalStorage.ToString());
        }

        /// <summary>
        /// Configure an alternative snapshot store
        /// </summary>
        /// <param name="factory"></param>
        public void SetSnapshotStoreFactory(Func<EngineConfiguration, ISnapshotStore> factory)
        {
            Register(args => factory.Invoke(this));
        }

        /// <summary>
        /// Rollover strategy is used by storage providers that split the journal into segments. The rollover strategy decides
        /// when to create a new segment.
        /// </summary>
        /// <returns></returns>
        public virtual RolloverStrategy CreateRolloverStrategy()
        {
            var compositeStrategy = new CompositeRolloverStrategy();

            if (MaxBytesPerJournalSegment < int.MaxValue)
            {
                compositeStrategy.AddStrategy(new MaxBytesRolloverStrategy(MaxBytesPerJournalSegment));
            }

            if (MaxEntriesPerJournalSegment < int.MaxValue)
            {
                compositeStrategy.AddStrategy(new MaxEntriesRolloverStrategy(MaxEntriesPerJournalSegment));
            }
            return compositeStrategy;
        }

        public virtual Kernel CreateKernel(Model model)
        {
            string registrationName = Kernel.ToString();
            var args = new TeenyIoc.Args { { "model", model } };
            return Registry.Resolve<Kernel>(args, registrationName);
        }

        /// <summary>
        /// Get the JournalPath
        /// </summary>
        /// <returns></returns>
        public string GetJournalPath()
        {
            return MakeAbsolute(JournalPath ?? ModelType.Name);
        }

        /// <summary>
        /// Get the directory for snapshots. Only relevant when using FileCommandStore
        /// </summary>
        /// <param name="makeAbsolute">Passes the result through the MakeAbsolute method. default is true</param>
        /// <returns></returns>
        public string GetSnapshotPath(bool makeAbsolute = true)
        {
            string path = SnapshotPath ?? JournalPath;
            if (makeAbsolute) path = MakeAbsolute(path);
            return path;
        }

        /// <summary>
        /// Make absolute by combining with GetDefaultDirectory()
        /// </summary>
        public static string MakeAbsolute(string path)
        {
            return Path.Combine(GetBaseDirectory(), path);
        }

        /// <summary>
        /// True if snapshots are stored in at path different from JournalPath
        /// </summary>
        /// <returns></returns>
        public bool HasAlternativeSnapshotPath()
        {
            return SnapshotPath != null && SnapshotPath != JournalPath;
        }

        /// <summary>
        /// The type of the model. Must be set when JournalPath is null
        /// because path is derived from type name.
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        /// Either current directory or App_Data when running in a web context
        /// </summary>
        public static string GetBaseDirectory()
        {
            string result = Directory.GetCurrentDirectory();

            String dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as String;

            if (dataDirectory != null && dataDirectory.EndsWith("App_Data"))
            {
                result = dataDirectory;
            }
            return result;
        }

        /// <summary>
        /// Configure this instance for use with an immutable kernel.
        /// </summary>
        /// <returns></returns>
        public EngineConfiguration ForImmutability()
        {
            Kernel = Kernels.Immutability;
            Synchronization = SynchronizationMode.None;
            Isolation = IsolationSettings.ForImmutability();
            return this;
        }
    }
}
