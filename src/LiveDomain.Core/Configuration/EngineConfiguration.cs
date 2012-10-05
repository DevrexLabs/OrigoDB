using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LiveDomain.Core.Logging;
using LiveDomain.Core.Security;
using LiveDomain.Core.TinyIoC;

namespace LiveDomain.Core
{

    public partial class EngineConfiguration : ConfigurationBase
    {
        protected TinyIoCContainer _registry;

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        public const string DefaultDateFormatString = "yyyy.MM.dd.hh.mm.ss.fff";
        public const int DefaultMaxBytesPerJournalSegment = 1024 * 1024 * 8;
        public const int DefaultMaxCommandsPerJournalSegment = 10000;


        /// <summary>
        /// Write journal entries using a background thread
        /// </summary>
        public bool AsyncronousJournaling { get; set; }


        /// <summary>
        /// Engine takes responsibility for ensuring no mutable object references are returned by command or queries. Default is true.
        /// <remarks>
        /// Can safely be set to false if one of the following is true:
        ///    1. You are running on a single thread and are certain that client code only reads results.
        ///    2. You have designed every single query and command to not return any mutable object references
        ///</remarks>
        /// </summary>
        public bool EnsureResultsAreDisconnected { get; set; }

        /// <summary>
        /// Make a deep copy of each command prior to execution. This will force a fast 
        /// failure of commands that wont serialize.
        /// </summary>
        public bool CloneCommands { get; set; }


        /// <summary>
        /// Maximum time to wait for any read or write lock
        /// </summary>
        public TimeSpan LockTimeout { get; set; }


        /// <summary>
        /// When to take automatic snapshots
        /// </summary>
        public SnapshotBehavior SnapshotBehavior { get; set; }

        public StoreType StoreType { get; set; }

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

        /// <summary>
        /// Create an EngineConfiguration instance using default values
        /// </summary>
        /// <param name="targetLocation"></param>
        public EngineConfiguration(string targetLocation = null)
        {

            Location = targetLocation;

            //Set default values
            LockTimeout = DefaultTimeout;
            Synchronization = SynchronizationMode.ReadWrite;
            ObjectFormatting = ObjectFormatting.NetBinaryFormatter;
            AsyncronousJournaling = false;
            MaxBytesPerJournalSegment = DefaultMaxBytesPerJournalSegment;
            MaxEntriesPerJournalSegment = DefaultMaxCommandsPerJournalSegment;
            StoreType = StoreType.FileSystem;
            EnsureResultsAreDisconnected = true;
            CloneCommands = true;

            _registry = new TinyIoCContainer();
            _registry.Register<ICommandJournal>((c, p) => new CommandJournal(this));
            _registry.Register<IAuthorizer<Type>>((c, p) => new TypeBasedPermissionSet(Permission.Allowed));
            _registry.Register<ISerializer>((c,p) => new Serializer(CreateFormatter()));

            InitSynchronizers();
            InitStoreTypes();
            InitFormatters();
        }

        #region Factory initializers
        /// <summary>
        /// Created a named registration for each SynchronizationMode enumeration value
        /// </summary>
        private void InitSynchronizers()
        {
            _registry.Register<ISynchronizer>((c, p) => new ReadWriteSynchronizer(LockTimeout),
                                              SynchronizationMode.ReadWrite.ToString());
            _registry.Register<ISynchronizer>((c, p) => new NullSynchronizer(),
                                              SynchronizationMode.None.ToString());
            _registry.Register<ISynchronizer>((c, p) => new ExclusiveSynchronizer(LockTimeout),
                                  SynchronizationMode.Exclusive.ToString());


        }

        private void InitFormatters()
        {
            _registry.Register<IFormatter>((c, p) => new BinaryFormatter(),
                                           ObjectFormatting.NetBinaryFormatter.ToString());
            _registry.Register<IFormatter>((c, p) => LoadFromConfig<IFormatter>(),
                                           ObjectFormatting.Custom.ToString());
        }


        /// <summary>
        /// Create a named registration for each StoreMode enumeration value
        /// </summary>
        private void InitStoreTypes()
        {
            _registry.Register<IStore>((c, p) => new FileStore(this), StoreType.FileSystem.ToString());
            //_registry.Register<IStorage, NullStorage>(StorageType.None.ToString());

            //If StorageMode is set to custom and no factory has been injected, the fully qualified type 
            //name will be resolved from the app configuration file.
            _registry.Register<IStore>((c, p) => LoadFromConfig<IStore>(), StoreType.Custom.ToString());
        } 
        #endregion

        /// <summary>
        /// Looks up a custom implementation in app config file with the key LiveDb.EngineConfiguration
        /// </summary>
        /// <returns></returns>
        public static EngineConfiguration Create()
        {
            //we need an instance to be able to call an instance method
            var bootloader = new EngineConfiguration();

            //look for specific implementation in config file, otherwise return self
            return bootloader.LoadFromConfigOrDefault(() => bootloader);
        }

        #region Factory methods
        public virtual ISerializer CreateSerializer()
        {
            return _registry.Resolve<ISerializer>();
        }

        public virtual IFormatter CreateFormatter()
        {
            string name = ObjectFormatting.ToString();
            return _registry.Resolve<IFormatter>(name);
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
            return _registry.Resolve<IStore>(name);
        }

        /// <summary>
        /// Creates and returns a new command journal instance, can only be called once.
        /// The default type is CommandJournal unless a custom factory has been set by 
        /// calling SetCommandJournalFactory()
        /// </summary>
        /// <returns></returns>
        public virtual ICommandJournal CreateCommandJournal()
        {
            return _registry.Resolve<ICommandJournal>();
        }

        #endregion

        #region Factory Injection Methods
        /// <summary>
        /// Inject a custom command journal factory. 
        /// Will throw if called after journal has been created.
        /// </summary>
        /// <param name="factory"></param>
        public void SetCommandJournalFactory(Func<EngineConfiguration, ICommandJournal> factory)
        {
            _registry.Register<ICommandJournal>((c, p) => factory.Invoke(this));
        }

        public void SetSynchronizerFactory(Func<EngineConfiguration, ISynchronizer> factory)
        {
            Synchronization = SynchronizationMode.Custom;
            string registrationName = Synchronization.ToString();
            _registry.Register<ISynchronizer>((c, p) => factory.Invoke(this), registrationName);
        }

        public void SetAuthorizerFactory(Func<EngineConfiguration, IAuthorizer<Type>> factory)
        {
            _registry.Register<IAuthorizer<Type>>((c, p) => factory.Invoke(this));
        }

        public void SetFormatterFactory(Func<EngineConfiguration, IFormatter> factory)
        {
            ObjectFormatting = ObjectFormatting.Custom;
            string registrationName = ObjectFormatting.ToString();
            _registry.Register<IFormatter>((c, p) => factory.Invoke(this), registrationName);
        }

        /// <summary>
        /// Inject your custom storage factory here. StorageMode property will be set to Custom
        /// </summary>
        /// <param name="factory"></param>
        public void SetStoreFactory(Func<EngineConfiguration, IStore> factory)
        {
            StoreType = StoreType.Custom;
            string registrationName = StoreType.ToString();
            _registry.Register<IStore>((c, p) => factory.Invoke(this), registrationName);
        }

        /// <summary>
        /// Inject a custom serializer factory
        /// </summary>
        /// <param name="factory"></param>
        public void SetSerializerFactory(Func<EngineConfiguration, ISerializer> factory)
        {
            _registry.Register<ISerializer>((c, p) => factory.Invoke(this));
        } 
        #endregion

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
    }
}
