using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace LiveDomain.Core
{

    [Serializable]
    public class EngineConfiguration
    {

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        public const string DefaultDateFormatString =  "yyyy.MM.dd.hh.mm.ss.fff";
        public const long DefaultJournalSegmentSizeInBytes = 1024 * 1024;


        /// <summary>
        /// Identifies where the command journal 
        /// </summary>
        public string Location{ get; set; }

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

        string _snapshotLocation = null;

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
        /// failure of commands that wont serialize. Default is false in RELEASE, true otherwise
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

        internal IJournalWriter CreateJournalWriter(Stream stream)
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

        internal IStorage CreateStorage()
        {
            return new FileStorage(this);
        }


        internal ICommandJournal CreateCommandJournal(IStorage _storage)
        {
            return new CommandJournal(this, _storage);
        }

        #endregion


        public static string GetDefaultLocation()
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
                return !String.IsNullOrEmpty(Location);
            }
        }

        public void SetDefaultLocation<M>() where M : Model
        {
            SetDefaultLocation(typeof(M));
            
        }

        public void SetDefaultLocation(Type type)
        {
            SetDefaultLocation(type.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the location</param>
        public void SetDefaultLocation(string name)
        {
            Location = GetDefaultLocation() + @"\" + name;
        }
    }

}
