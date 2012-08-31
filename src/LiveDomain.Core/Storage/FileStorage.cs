using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using LiveDomain.Core.Storage;

namespace LiveDomain.Core
{
    /// <summary>
    /// File system based Storage implementation
    /// </summary>
    public class FileStorage : IStorage
    {

        EngineConfiguration _config;
        ISerializer _serializer;

        private List<FileSnapshot> _fileSnapshots;
        private List<JournalFile> _journalFiles; 


        public IEnumerable<Snapshot> Snapshots
        {
            get
            {
                foreach (var snapshot in Snapshots)
                {
                    yield return snapshot;
                }
            }
        }


        //public static FileStorage Create(EngineConfiguration config, Model initialModel)
        //{
        //    var storage = new FileStorage(config);
        //    storage.Create(initialModel);
        //    return storage;
        //}

        //public static FileStorage Load(EngineConfiguration config)
        //{
        //    var storage = new FileStorage(config);
        //    storage.LoadMetadata();
        //    return storage;
        //}

        public FileStorage(EngineConfiguration config)
        {
            _config = config;
            _serializer = _config.CreateSerializer();
        }


        /// <summary>
        /// Read physical storage and populate metadata collections
        /// </summary>
        public void Load()
        {
            _journalFiles = new List<JournalFile>();
            foreach (var file in Directory.GetFiles(_config.Location, "*.journal"))
            {
                string fileName = new FileInfo(file).Name;
                _journalFiles.Add(JournalFile.Parse(fileName));
            }

            _journalFiles.Sort((a,b) => a.FileSequenceNumber.CompareTo(b.FileSequenceNumber));

            _fileSnapshots = new List<FileSnapshot>();
            foreach (var file in Directory.GetFiles(_config.SnapshotLocation, "*.snapshot"))
            {
                var fileInfo = new FileInfo(file);
                _fileSnapshots.Add(FileSnapshot.FromFileInfo(fileInfo));
            }

            _fileSnapshots.Sort((a,b) => a.LastSequenceNumber.CompareTo(b.LastSequenceNumber));
        }


        public void WriteSnapshot(Model model, long lastEntryId)
        {
            //TODO: unused datetime is smelly. refactor.
            var fileSnapshot = new FileSnapshot(DateTime.MinValue, lastEntryId);
            var fileName = Path.Combine(_config.SnapshotLocation, fileSnapshot.Name);
            using (Stream stream = GetWriteStream(fileName, append:false))
            {
                _serializer.Write(model, stream);
            }
        }

        public IEnumerable<JournalEntry<Command>> GetJournalEntries()
        {
            return GetJournalEntriesFrom(1);
        }


        public IEnumerable<JournalEntry<Command>> GetJournalEntriesFrom(long sequenceNumber)
        {
            int offset = 0;
            foreach (var journalFile in _journalFiles)
            {
                if (journalFile.StartingSequenceNumber > sequenceNumber) break;
                offset++;
            }
            //_journalFiles
            //        .Where((jf, idx) => jf.StartingSequenceNumber <= sequenceNumber)
            //        .Select((jf, idx) => idx).
            //        LastOrDefault();

            foreach (var journalFile in _journalFiles.Skip(offset))
            {
                string path = Path.Combine(_config.Location, journalFile.Name);
                using (Stream stream = GetReadStream(path))
                {
                    foreach (var entry in _serializer.ReadToEnd<JournalEntry<Command>>(stream))
                    {
                        if (entry.Id < sequenceNumber) continue;
                        yield return entry;
                    }
                }
            }

        }

        public IEnumerable<JournalEntry<Command>> GetJournalEntriesFrom(DateTime pointInTime)
        {
            //TODO: implement this when together with point in time recovery
            throw new NotImplementedException();
        }



        public Model LoadMostRecentSnapshot(out long lastSequenceNumber)
        {
            lastSequenceNumber = 0;
            var snapshot = _fileSnapshots.LastOrDefault();
            if (snapshot == null) return null;
            var directory = _config.SnapshotLocation;
            var fileName = Path.Combine(directory, snapshot.Name);
            lastSequenceNumber = snapshot.LastSequenceNumber;
            using (Stream stream = GetReadStream(fileName))
            {
                return _serializer.Read<Model>(stream);
            }
        }

        /// <summary>
        /// Create a new journal writer stream. The first entry written to the 
        /// stream will have the specified sequenceNumber
        /// </summary>
        /// <returns>An open stream</returns>
        public Stream CreateJournalWriterStream(long firstEntryId = 1)
        {
            var journalFile = _journalFiles.LastOrDefault() ?? new JournalFile(0, 0);
            string fileName = journalFile.Successsor(firstEntryId).Name;
            string path = Path.Combine(_config.Location, fileName);
            return GetWriteStream(path,  append : true);
        }

        public void Create(Model initialModel)
        {
            VerifyCanCreate();
            EnsureDirectoryExists(_config.Location);

            if (_config.HasAlternativeSnapshotLocation)
            {
                EnsureDirectoryExists(_config.SnapshotLocation);
            }
            WriteSnapshot(initialModel, 0);
            Load();
        }


        bool IStorage.Exists
        {
            get 
            {
                try
                {
                    VerifyCanLoad();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool CanCreate
        {
            get
            {
                try
                {
                    VerifyCanCreate();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }



        private void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        protected void RemoveSnapshot(string id)
        {
            string path = Path.Combine(_config.SnapshotLocation, id);
            File.Delete(path);
        }

        private Stream GetReadStream(string fileName)
        {
            return File.OpenRead(fileName);
        }

        protected  bool Exists(string path)
        {
            return File.Exists(path);
        }

        protected  Stream GetWriteStream(string path, bool append)
        {
            var filemode = append ? FileMode.Append : FileMode.Create;
            return new FileStream(path, filemode, FileAccess.Write);
        }

        /// <summary>
        /// Must not exist but must be empty if it does
        /// </summary>
        /// <param name="directory"></param>
        private void VerifyDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                if (!DirectoryEx.IsEmpty(directory))
                {
                    throw new ApplicationException("Directory must be empty: " + directory);
                }
            }
        }

        public void VerifyCanCreate()
        {
            VerifyDirectory(_config.Location);
            if (_config.HasAlternativeSnapshotLocation)
                VerifyDirectory(_config.SnapshotLocation);
        }

        public void VerifyCanLoad()
        {
            string error = String.Empty;
            if (!Directory.Exists(_config.Location))
            {
                error = "Target directory does not exist\n";
            }
            //else if (!Directory.GetFiles(_config.Location, "*.journal").Any())
            //{
            //    error += "No journal files found in target directory\n";
            //}


            if (_config.HasAlternativeSnapshotLocation)
            {
                if (!Directory.Exists(_config.SnapshotLocation))
                {
                    error += "Snapshot directory does not exist\n";
                }
            }

            string initialSnapshot = Path.Combine(_config.SnapshotLocation, "000000000.snapshot");
            if (!File.Exists(initialSnapshot))
            {
                error += "Initial snapshot missing\n";
            }

            if (error != String.Empty)
            {
                throw new ApplicationException("Error(s) loading: " + error);   
            }
        }

        public IJournalWriter CreateJournalWriter(long lastEntryId)
        {

            var compositeStrategy = new CompositeRolloverStrategy();
            
            if (_config.MaxBytesPerJournalSegment < int.MaxValue)
            {
                compositeStrategy.AddStrategy(new MaxBytesRolloverStrategy(_config.MaxBytesPerJournalSegment));
            }
            
            if (_config.MaxEntriesPerJournalSegment < int.MaxValue)
            {
                compositeStrategy.AddStrategy(new MaxEntriesRolloverStrategy(_config.MaxEntriesPerJournalSegment));
            }
            
            return new StreamJournalWriter(this, CreateJournalWriterStream(lastEntryId), _config, compositeStrategy);
        }
    }
}
