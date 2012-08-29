using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LiveDomain.Core.Logging;
using LiveDomain.Core.Security;
using TinyIoC;

namespace LiveDomain.Core
{

    public partial class EngineConfiguration : ConfigurationBase
    {

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        public const string DefaultDateFormatString = "yyyy.MM.dd.hh.mm.ss.fff";
        public const long DefaultJournalSegmentSizeInBytes = 1024 * 1024;


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


        /// <summary>
        /// When to take automatic snapshots
        /// </summary>
        public SnapshotBehavior SnapshotBehavior { get; set; }

        public StorageMode StorageMode { get; set; }

        public SynchronizationMode Concurrency { get; set; }


        public SerializationMethod SerializationMethod { get; set; }
        public JournalWriterPerformanceMode JournalWriterPerformance { get; set; }


        /// <summary>
        /// Create an EngineConfiguration instance using default values
        /// </summary>
        /// <param name="targetLocation"></param>
        public EngineConfiguration(string targetLocation = null)
        {

            Location = targetLocation;

            //Set default values
            LockTimeout = DefaultTimeout;
            Concurrency = SynchronizationMode.SharedRead;
            SerializationMethod = SerializationMethod.NetBinaryFormatter;
            JournalWriterPerformance = JournalWriterPerformanceMode.Synchronous;
            StorageMode = StorageMode.FileSystem;
            JournalSegmentSizeInBytes = DefaultJournalSegmentSizeInBytes;
            CloneResults = true;
            CloneCommands = true;

            _registry.Register<ICommandJournal>((c, p) => new CommandJournal(this, CreateStorage()));
            _registry.Register<IAuthorizer<Type>>((c, p) => new PermissionSet<Type>(Permission.Allowed));
            _registry.Register<ISerializer>((c,p) => new Serializer(CreateFormatter()));

            InitJournalWriterConfiguration();
            InitStorageConfiguration();
            InitLockingConfiguration();
            InitFormatterConfiguration();
        }

        private void InitJournalWriterConfiguration()
        {
            _registry.Register<IJournalWriter>(
                (c, p) => new AsynchronousJournalWriter(new SynchronousJournalWriter(p["stream"] as Stream, CreateSerializer())),
                JournalWriterPerformanceMode.Asynchronous.ToString());
            _registry.Register<IJournalWriter>(
                (c, p) => new SynchronousJournalWriter(p["stream"] as Stream, CreateSerializer()),
                JournalWriterPerformanceMode.Synchronous.ToString());
        }

        private void InitFormatterConfiguration()
        {
            _registry.Register<IFormatter>((c, p) => new BinaryFormatter(),
                                           SerializationMethod.NetBinaryFormatter.ToString());
            _registry.Register<IFormatter>((c, p) => LoadFromConfig<IFormatter>(),
                                           SerializationMethod.Custom.ToString());
        }


        /// <summary>
        /// Created a named registration for each ConcurrencyMode enumeration value
        /// </summary>
        private void InitLockingConfiguration()
        {
            _registry.Register<ILockStrategy, SingleThreadedLockingStrategy>(SynchronizationMode.Exclusive.ToString());
            _registry.Register<ILockStrategy, ReaderWriterLockSlimStrategy>(SynchronizationMode.SharedRead.ToString());
            _registry.Register<ILockStrategy, NullLockingStrategy>(SynchronizationMode.None.ToString());
        }

        /// <summary>
        /// Create a named registration for each StorageMode enumeration value
        /// </summary>
        private void InitStorageConfiguration()
        {
            
            _registry.Register<IStorage>((c, p) => new FileStorage(this), StorageMode.FileSystem.ToString());
            _registry.Register<IStorage, NullStorage>(StorageMode.None.ToString());

            //If StorageMode is set to custom and no factory has been injected, the fully qualified type 
            //name will be resolved from the app configuration file.
            _registry.Register<IStorage>((c, p) => LoadFromConfig<IStorage>(), StorageMode.Custom.ToString());
        }


        protected TinyIoCContainer _registry = new TinyIoCContainer();

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

        /// <summary>
        /// Inject your custom storage factory here. StorageMode property will be set to Custom
        /// </summary>
        /// <param name="factory"></param>
        public void SetCustomStorageFactory(Func<IStorage> factory)
        {
            StorageMode = StorageMode.Custom;
            _registry.Register<IStorage>((c, p) => factory.Invoke(), StorageMode.Custom.ToString());
        }

        protected internal virtual ISerializer CreateSerializer()
        {
            return _registry.Resolve<ISerializer>();
        }

        public void SetCustomSerializerFactory(Func<EngineConfiguration, ISerializer> factory )
        {
            _registry.Register<ISerializer>((c, p) => factory.Invoke(this));
        }
        protected internal virtual IFormatter CreateFormatter()
        {
            string name = SerializationMethod.ToString();
            return _registry.Resolve<IFormatter>(name);
        }

        public void SetCustomFormatterFactory(Func<EngineConfiguration, IFormatter> factory )
        {
            _registry.Register<IFormatter>((c, p) => factory.Invoke(this));
        }

        /// <summary>
        /// Factory method choosing concrete type based on ConcurrencyMode property
        /// </summary>
        /// <returns></returns>
        protected internal virtual ILockStrategy CreateLockingStrategy()
        {
            return _registry.Resolve<ILockStrategy>(Concurrency.ToString());
        }

        protected internal virtual IJournalWriter CreateJournalWriter(Stream stream)
        {
            string registrationName = JournalWriterPerformance.ToString();
            var args = new NamedParameterOverloads {{"stream", stream}};
            return _registry.Resolve<IJournalWriter>(registrationName, args);
        }


        protected internal virtual IAuthorizer<Type> GetAuthorizer()
        {
            return _registry.Resolve<IAuthorizer<Type>>();
        }



        protected internal virtual IStorage CreateStorage()
        {
            string name = StorageMode.ToString();
            return _registry.Resolve<IStorage>(name);
        }

        private bool _commandJournalCreated = false;


        /// <summary>
        /// Creates and returns a new command journal instance, can only be called once.
        /// The default type is CommandJournal unless a custom factory has been set by 
        /// calling SetCommandJournalFactory()
        /// </summary>
        /// <returns></returns>
        protected internal virtual ICommandJournal CreateCommandJournal()
        {
            if(_commandJournalCreated) throw new InvalidOperationException();
            _commandJournalCreated = true;
            return _registry.Resolve<ICommandJournal>();
        }

        /// <summary>
        /// Inject a custom command journal factory. 
        /// Will throw if called after journal has been created.
        /// </summary>
        /// <param name="factory"></param>
        public void SetCommandJournalFactory(Func<ICommandJournal> factory)
        {
            if (_commandJournalCreated) throw new InvalidOperationException();
            _registry.Register<ICommandJournal>((c, p) => factory.Invoke());
        }
    }
}
