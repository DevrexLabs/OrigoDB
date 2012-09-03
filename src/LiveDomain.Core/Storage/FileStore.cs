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
    public sealed class FileStore : Store
    {
        private List<JournalFile> _journalFiles;

        public IEnumerable<JournalFile> JournalFiles
        {
            get
            {
                foreach (var journalFile in _journalFiles) yield return journalFile;
            }
        }

        public FileStore(EngineConfiguration config) : base(config)
        {
        }


        /// <summary>
        /// Read journal files
        /// </summary>
        public override void Load()
        {
            base.Load();
            _journalFiles = new List<JournalFile>();
            foreach (var file in Directory.GetFiles(_config.Location, "*.journal"))
            {
                string fileName = new FileInfo(file).Name;
                _journalFiles.Add(JournalFile.Parse(fileName));
            }

            _journalFiles.Sort((a,b) => a.FileSequenceNumber.CompareTo(b.FileSequenceNumber));
        }


        protected override Snapshot WriteSnapshotImpl(Model model, long lastEntryId)
        {
            var fileSnapshot = new FileSnapshot(DateTime.Now, lastEntryId);
            var fileName = Path.Combine(_config.SnapshotLocation, fileSnapshot.Name);
            using (Stream stream = GetWriteStream(fileName, append:false))
            {
                _serializer.Write(model, stream);
            }
            return fileSnapshot;
        }

        public override IEnumerable<JournalEntry<Command>> GetJournalEntriesFrom(long sequenceNumber)
        {
            int offset = 0;
            foreach (var journalFile in _journalFiles)
            {
                if (journalFile.StartingEntryId >= sequenceNumber) break;
                offset++;
            }

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

        public override IEnumerable<JournalEntry<Command>> GetJournalEntriesBeforeOrAt(DateTime pointInTime)
        {
            foreach (var journalEntry in GetJournalEntries())
            {
                if (journalEntry.Created <= pointInTime) yield return journalEntry;
            }
        }



        public override Model LoadMostRecentSnapshot(out long lastSequenceNumber)
        {
            lastSequenceNumber = 0;
            FileSnapshot snapshot = (FileSnapshot) Snapshots.LastOrDefault();
            if (snapshot == null) return null;
            var directory = _config.SnapshotLocation;
            var fileName = Path.Combine(directory, snapshot.Name);
            lastSequenceNumber = snapshot.LastEntryId;
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
            var current = _journalFiles.LastOrDefault() ?? new JournalFile(0, 0);
            var next = current.Successsor(firstEntryId);
            _journalFiles.Add(next);
            string fileName = next.Name;
            string path = Path.Combine(_config.Location, fileName);
            return GetWriteStream(path,  append : true);
        }

        public override void Create(Model initialModel)
        {
            VerifyCanCreate();
            EnsureDirectoryExists(_config.Location);

            if (_config.HasAlternativeSnapshotLocation)
            {
                EnsureDirectoryExists(_config.SnapshotLocation);
            }
            Load();
            WriteSnapshot(initialModel, 0);
            
        }


        private void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }


        private Stream GetReadStream(string fileName)
        {
            return File.OpenRead(fileName);
        }


        private  Stream GetWriteStream(string path, bool append)
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

        public override void VerifyCanCreate()
        {
            VerifyDirectory(_config.Location);
            if (_config.HasAlternativeSnapshotLocation)
                VerifyDirectory(_config.SnapshotLocation);
        }

        public override void VerifyCanLoad()
        {
            string error = String.Empty;
            if (!Directory.Exists(_config.Location))
            {
                error = "Target directory does not exist\n";
            }

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

        protected override IJournalWriter CreateStoreSpecificJournalWriter(long lastEntryId)
        {
            return new StreamJournalWriter(this, _config);
        }

        protected override IEnumerable<Snapshot> LoadSnapshots()
        {
            var snapshots = new List<FileSnapshot>();
            foreach (var file in Directory.GetFiles(_config.SnapshotLocation, "*.snapshot"))
            {
                var fileInfo = new FileInfo(file);
                snapshots.Add(FileSnapshot.FromFileInfo(fileInfo));
            }

            snapshots.Sort((a, b) => a.LastEntryId.CompareTo(b.LastEntryId));
            return snapshots;
        }
    }
}
