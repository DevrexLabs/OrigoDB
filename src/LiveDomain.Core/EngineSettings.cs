using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace LiveDomain.Core
{

    public enum SerializationMethod
    {
        NetBinaryFormatter
    }

    public enum CompressionMethod
    {
        /// <summary>
        /// Default, no compression
        /// </summary>
        None,
        GZip
        //TODO: Add 7zip support
    }

    public enum JournalWriterPerformanceMode
    {
        /// <summary>
        /// Safe but slower
        /// Journal writer waits for disc write and flush, exceptions are propagated to client.
        /// </summary>
        Synchronous,

        /// <summary>
        /// High Performance but unsafe, commands can get lost.
        /// Journal writer runs in the background, disk write time is not included in command execution time.
        /// </summary>
        Asynchronous
    }

    public enum ConcurrencyMode
    {
        /// <summary>
        /// Allow access to one thread at a time for either reading or writing
        /// </summary>
        SingleReaderOrWriter,

        /// <summary>
        /// Allow multiple reader threads or 
        /// </summary>
        MultipleReadersOrSingleWriter,

        /// <summary>
        /// Allow any access, thread safety is controlled by client code
        /// </summary>
        MultipleReadersAndWriters
    }

    public class EngineSettings
    {

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Root directory where model, journal and snapshots are stored.
        /// </summary>
        public string Path { get; set; }
        

        /// <summary>
        /// Maximum time to wait for any read or write lock
        /// </summary>
        public TimeSpan LockTimeout { get; set; }

        public CompressionMethod Compression { get; set; }
        public ConcurrencyMode Concurrency{ get; set; }
        public SerializationMethod SerializationMethod { get; set; }
        public JournalWriterPerformanceMode JournalWriterPerformance { get; set; }

        public EngineSettings(string path)
        {
            Path = path;

            //Set default values
            LockTimeout = DefaultTimeout;
            Compression = CompressionMethod.None;
            Concurrency = ConcurrencyMode.MultipleReadersOrSingleWriter;
            SerializationMethod = SerializationMethod.NetBinaryFormatter;
            JournalWriterPerformance = JournalWriterPerformanceMode.Synchronous;
        }


        /// <summary>
        /// Factory method choosing concrete type based on SerializationMethod property
        /// </summary>
        /// <returns></returns>

        internal IFormatter CreateSerializationFormatter()
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

        internal IJournalWriter CreateJournalWriter(Stream stream, Serializer serializer)
        {

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
    }
}
