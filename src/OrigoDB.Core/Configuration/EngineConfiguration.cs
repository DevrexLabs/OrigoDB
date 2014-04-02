using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using OrigoDB.Core.Security;
using OrigoDB.Core.Configuration;

namespace OrigoDB.Core
{

    public class EngineConfiguration : ConfigurationBase
    {
        protected TeenyIoc _registry;

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        public const string DefaultDateFormatString = "yyyy.MM.dd.hh.mm.ss.fff";
        public const int DefaultMaxBytesPerJournalSegment = 1024 * 1024 * 8;
        public const int DefaultMaxCommandsPerJournalSegment = 10000;



        public EngineConfiguration ForImmutability()
        {
            Kernel = Kernels.Immutability;
            Synchronization = SynchronizationMode.None;
            EnsureSafeResults = false;
            return this;
        }


        /// <summary>
        /// Write journal entries using a background thread
        /// </summary>
        public bool AsyncronousJournaling { get; set; }

        public PersistenceMode PersistenceMode { get; set; }

        public Kernels Kernel { get; set; }

        /// <summary>
        /// If assigned, write <see cref="Packet"/>s to the journal using the options specified.
        /// Otherwise serialize object graphs directly to the underlying stream
        /// </summary>
        public PacketOptions? PacketOptions { get; set; }

        /// <summary>
        /// Engine takes responsibility for ensuring no mutable object references are returned
        /// by commands or queries. Default is true.
        /// <remarks>
        /// Can safely be set to false if one of the following is true:
        ///    1. You are running on a single thread and are certain that client code only reads results.
        ///    2. You have designed every single query and command to not return any references to mutable objects
        ///</remarks>
        /// </summary>
        public bool EnsureSafeResults { get; set; }


        /// <summary>
        /// Maximum time to wait for any read or write lock
        /// </summary>
        public TimeSpan LockTimeout { get; set; }


        /// <summary>
        /// When to take automatic snapshots
        /// </summary>
        public SnapshotBehavior SnapshotBehavior { get; set; }

        public Stores StoreType { get; set; }

        /// <summary>
        /// Effects which ISynchronizer is chosen by CreateSynchronizer()
        /// </summary>
        public SynchronizationMode Synchronization { get; set; }

        /// <summary>
        /// Serialization format, defaults to BinaryFormatter
        /// </summary>
        public ObjectFormatting ObjectFormatting { get; set; }

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


        public StorageLocation Location { get; set; }

        /// <summary>
        /// Create an EngineConfiguration instance using default values
        /// </summary>
        /// <param name="targetLocation"></param>
        public EngineConfiguration(string targetLocation = null)
        {
            Location = new FileStorageLocation(targetLocation);

            //Set default values
            Kernel = Kernels.Optimistic;
            LockTimeout = DefaultTimeout;
            Synchronization = SynchronizationMode.ReadWrite;
            ObjectFormatting = ObjectFormatting.NetBinaryFormatter;
            AsyncronousJournaling = false;
            MaxBytesPerJournalSegment = DefaultMaxBytesPerJournalSegment;
            MaxEntriesPerJournalSegment = DefaultMaxCommandsPerJournalSegment;
            StoreType = Stores.FileSystem;
            EnsureSafeResults = true;
            PacketOptions = null;
            PersistenceMode = PersistenceMode.Journaling;

            _registry = new TeenyIoc();
            Register<IAuthorizer<Type>>(c => new TypeBasedPermissionSet(Permission.Allowed));
            Register<ISerializer>(c => new Serializer(CreateFormatter()));
            InitSynchronizers();
            InitStoreTypes();
            InitFormatters();
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

        private void InitFormatters()
        {
            Register<IFormatter>(c => new BinaryFormatter(),
                                           ObjectFormatting.NetBinaryFormatter.ToString());
            Register(c => LoadFromConfig<IFormatter>(),
                                           ObjectFormatting.Custom.ToString());
        }


        /// <summary>
        /// Create a named registration for each StoreMode enumeration value
        /// </summary>
        private void InitStoreTypes()
        {
            Register<IStore>(cfg => new FileStore(cfg), Stores.FileSystem.ToString());

            //If StorageMode is set to custom and no factory has been injected, the fully qualified type 
            //name will be resolved from the app configuration file.
            Register(c => LoadFromConfig<IStore>(), Stores.Custom.ToString());
        }

        /// <summary>
        /// Looks up a custom implementation in app config file
        /// </summary>
        /// <returns></returns>
        public static EngineConfiguration Create()
        {
            //we need an instance to be able to call an instance method
            var bootloader = new EngineConfiguration();

            //look for specific implementation in config file, otherwise return self
            return bootloader.LoadFromConfigOrDefault(() => bootloader);
        }

        public virtual ISerializer CreateSerializer()
        {
            return _registry.Resolve<ISerializer>();
        }

        public virtual IFormatter CreateFormatter()
        {
            string name = ObjectFormatting.ToString();
            var formatter = _registry.Resolve<IFormatter>(name: name);
            if (PacketOptions != null)
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
            return _registry.Resolve<ISynchronizer>(registrationName);
        }

        public virtual IAuthorizer<Type> CreateAuthorizer()
        {
            return _registry.Resolve<IAuthorizer<Type>>();
        }

        public virtual IStore CreateStore()
        {
            string name = StoreType.ToString();
            var store =  _registry.Resolve<IStore>(name);
            store.Init();
            return store;
        }


        protected void Register<T>(Func<EngineConfiguration, T> factory, string registrationName = "") where T : class
        {
            _registry.Register<T>(args => factory.Invoke(this), registrationName);
        }

        protected void Register<T>(Func<EngineConfiguration, TeenyIoc.Args, T> factory,
            string registrationName = "") where T : class
        {
            _registry.Register<T>(args => factory.Invoke(this, args), registrationName);
        }


        public void SetSynchronizerFactory(Func<EngineConfiguration, ISynchronizer> factory)
        {
            Synchronization = SynchronizationMode.Custom;
            string registrationName = Synchronization.ToString();
            _registry.Register<ISynchronizer>(args => factory.Invoke(this), registrationName);
        }

        public void SetAuthorizerFactory(Func<EngineConfiguration, IAuthorizer<Type>> factory)
        {
            Register(args => factory.Invoke(this));
        }

        public void SetFormatterFactory(Func<EngineConfiguration, IFormatter> factory)
        {
            ObjectFormatting = ObjectFormatting.Custom;
            string registrationName = ObjectFormatting.ToString();
            Register(args => factory.Invoke(this), registrationName);
        }

        /// <summary>
        /// Inject your custom storage factory here. StorageMode property will be set to Custom
        /// </summary>
        /// <param name="factory"></param>
        public void SetStoreFactory(Func<EngineConfiguration, IStore> factory)
        {
            StoreType = Stores.Custom;
            string registrationName = StoreType.ToString();
            Register(args => factory.Invoke(this), registrationName);
        }

        /// <summary>
        /// Inject a custom serializer factory
        /// </summary>
        /// <param name="factory"></param>
        public void SetSerializerFactory(Func<EngineConfiguration, ISerializer> factory)
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
            return _registry.Resolve<Kernel>(args, registrationName);
        }
    }
}
