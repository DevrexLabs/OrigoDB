using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using System.Runtime.Serialization;

namespace LiveDomain.Core
{
    /// <summary>
    /// Knows how to serialize, deserialize, read and write to disc.
    /// </summary>
    internal class Storage : IDisposable
    {

        const string DataFileName = "model.bin";
        const string LogFileName = "log.bin";

        //Private members
        Serializer _serializer;
        string _snapshotDirectory;
        string _logfile;
        EngineSettings _settings;

        internal string RootDirectory { get; private set; }

        private ILogWriter _logwriter;

        internal void AppendToLog(ILogCommand command)
        {
            try
            {
                _logwriter.Write(new LogItem(command));
            }
            catch (SerializationException)
            {
                //The command cannot be serialized
                throw;
            }
            catch (Exception ex)
            {
                throw new FatalException("Cannot write to the command log, see inner exception for details", ex);
            }
        }


        internal Storage(EngineSettings settings)
        {
            _settings = settings;
            RootDirectory = _settings.Path;
            _snapshotDirectory = Path.Combine(RootDirectory, "snapshots");
            _logfile = Path.Combine(_settings.AlternateLogPath ?? RootDirectory, LogFileName);
            _serializer = CreateSerializer();
            _logwriter = CreateLogWriter();

        }


        internal Model ReadModel()
        {
            string path = GetDataFilePath();
            var stream = File.OpenRead(path);
            using (stream)
            {
                return _serializer.Read<Model>(stream);
            }
        }

        /// <summary>
        /// Serializes and writes the model to disc. Will throw an exception if a snapshot with the same name exists.
        /// </summary>
        /// <param name="name">The name of the snapshot, must be a valid filename</param>
        /// <param name="model">The model to save</param>      
        internal void Save(Model model, string name)
        {
            if (!Regex.IsMatch(name, "^[a-zA-Z0-9_]+$")) 
                throw new ArgumentException("Snapshot name contains invalid chars. a-zA-Z0-9_", name);
            
            string path = _snapshotDirectory + "\\" + name;
            if (File.Exists(path)) 
                throw new InvalidOperationException("Snapshot name already exists");
            
            
            //Delegate to overloaded method
            WriteModel(path, model);
        }


        internal IEnumerable<SnapshotInfo> GetSnapshots()
        {
            return Directory.GetFiles(_snapshotDirectory)
                .Select(name => new FileInfo(Path.Combine(_snapshotDirectory , name)))
                .Select( s => new SnapshotInfo
                { 
                    Name = s.Name, 
                    Created = s.CreationTime, 
                    SizeOnDiscInBytes = s.Length 
                }).ToArray();
        }

        /// <summary>
        /// Replace the main serialized graph with the current version of the model
        /// </summary>
        /// <param name="model"></param>
        internal void Merge(Model model)
        {
            string path = GetDataFilePath();
            string tempPath = GetTempPath();
            
            WriteModel(tempPath, model);

            if (File.Exists(path)) File.Delete(path);

            File.Move(tempPath, path);

            TruncateLog();
        }

        internal void TruncateLog()
        {
            _logwriter.Dispose();
            File.Delete(GetLogFilePath());
            _logwriter = CreateLogWriter(); 
        }

        private ILogWriter CreateLogWriter()
        {
            Serializer serializer = CreateSerializer();
            Stream stream = GetWriteStream(_logfile);
            return _settings.CreateLogWriter(stream, serializer);
        }


        /// <summary>
        /// Serializes the model to the specified path. Assumes sufficient disc space and that the file is writeable.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="model"></param>
        private void WriteModel(string path, Model model)
        {
            Stream stream = GetWriteStream(path);
            using (stream)
            {
                _serializer.Write(model,stream);
            }
        }

        private string GetDataFilePath()
        {
            return RootDirectory + @"\" + DataFileName;
        }

        private string GetLogFilePath()
        {
            return RootDirectory + @"\" + LogFileName;
        }

        private string GetTempPath()
        {
            return RootDirectory + @"\" + Guid.NewGuid().ToString();
        }


        internal IEnumerable<LogItem> ReadLogEntries()
        {
            _logwriter.Dispose();
            Stream logStream = GetReadStream(GetLogFilePath());

            using (logStream)
            {
                foreach (var logItem in _serializer.ReadToEnd<LogItem>(logStream))
                {
                    yield return logItem;
                }
            }

            _logwriter = CreateLogWriter();
        }

        private Serializer CreateSerializer()
        {
            IFormatter formatter = _settings.CreateSerializationFormatter();
            return new Serializer(formatter);
        }

        protected Stream GetWriteStream(string path)
        {
            Stream stream = File.OpenWrite(path);
            if (_settings.Compression == CompressionMethod.GZip) stream = new GZipStream(stream, CompressionMode.Compress);
            return stream;
        }

        protected Stream GetReadStream(string path)
        {
            Stream stream = File.OpenRead(path);
            if (_settings.Compression == CompressionMethod.GZip) stream = new GZipStream(stream, CompressionMode.Decompress);
            return stream;
        }



        public void Dispose()
        {
            _logwriter.Dispose();
        }

        internal static Storage Create(string path)
        {
            return Create(new EngineSettings(path));
        }

        internal static Storage Create(EngineSettings settings)
        {
            string path = settings.Path;
            if (Directory.Exists(path)) throw new Exception("Directory already exists");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + @"\snapshots");
            return new Storage(settings);
        }
    }
}
