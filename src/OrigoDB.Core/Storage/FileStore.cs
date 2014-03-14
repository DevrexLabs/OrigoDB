using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core
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
            foreach (var file in Directory.GetFiles(_config.Location.OfJournal, "*.journal"))
            {
                string fileName = new FileInfo(file).Name;
                _journalFiles.Add(JournalFile.Parse(fileName));
            }

            _journalFiles.Sort((a,b) => a.FileSequenceNumber.CompareTo(b.FileSequenceNumber));
        }


        protected override Snapshot WriteSnapshotImpl(Model model)
        {
            var fileSnapshot = new FileSnapshot(DateTime.Now, LastEntryId);
            var fileName = Path.Combine(_config.Location.OfSnapshots, fileSnapshot.Name);
            using (Stream stream = GetWriteStream(fileName, append:false))
            {
                _serializer.Write(model, stream);
            }
            return fileSnapshot;
        }

        public override IEnumerable<JournalEntry> GetJournalEntriesFrom(ulong entryId)
        {
			if (entryId != 0 && entryId < _journalFiles[0].StartingEntryId)
				throw new NotSupportedException("Journal file missing");

            //Scroll to the correct file
            int offset = 0;
	        while (_journalFiles.Count > offset + 1 && _journalFiles[offset + 1].StartingEntryId < entryId)
				offset++;

            foreach (var journalFile in _journalFiles.Skip(offset))
            {
                string path = Path.Combine(_config.Location.OfJournal, journalFile.Name);
                using (Stream stream = GetReadStream(path))
                {
                    foreach (var entry in _serializer.ReadToEnd<JournalEntry>(stream))
                    {
                        if (entry.Id < entryId) continue;
                        yield return entry;
                    }
                }
            }

        }

        public override IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime)
        {
            foreach (var journalEntry in GetJournalEntries())
            {
                if (journalEntry.Created <= pointInTime) yield return journalEntry;
            }
        }



        public override Model LoadMostRecentSnapshot(out ulong lastSequenceNumber)
        {
            lastSequenceNumber = 0;
            FileSnapshot snapshot = (FileSnapshot) Snapshots.LastOrDefault();
            if (snapshot == null) return null;
            var directory = _config.Location.OfSnapshots;
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
        public override Stream CreateJournalWriterStream(ulong firstEntryId = 1)
        {
            var current = _journalFiles.LastOrDefault() ?? new JournalFile(0, 0);
            var next = current.Successor(firstEntryId);
            _journalFiles.Add(next);
            string fileName = next.Name;
            string path = Path.Combine(_config.Location.OfJournal, fileName);
            return GetWriteStream(path,  append : true);
        }

        public override void Create(Model initialModel)
        {
            VerifyCanCreate();
            EnsureDirectoryExists(_config.Location.OfJournal);

            if (_config.Location.HasAlternativeSnapshotLocation)
            {
                EnsureDirectoryExists(_config.Location.OfSnapshots);
            }
            Load();
            LastEntryId = 0;
            WriteSnapshot(initialModel);
            
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
	        return File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
            VerifyDirectory(_config.Location.OfJournal);
            if (_config.Location.HasAlternativeSnapshotLocation)
                VerifyDirectory(_config.Location.OfSnapshots);
        }

        public override void VerifyCanLoad()
        {
            string error = String.Empty;
            if (!Directory.Exists(_config.Location.OfJournal))
            {
                error = "Target directory does not exist\n";
            }

            if (_config.Location.HasAlternativeSnapshotLocation)
            {
                if (!Directory.Exists(_config.Location.OfSnapshots))
                {
                    error += "Snapshot directory does not exist\n";
                }
            }

            string initialSnapshot = Path.Combine(_config.Location.OfSnapshots, "000000000.snapshot");
            if (!File.Exists(initialSnapshot))
            {
                error += "Initial snapshot missing\n";
            }

            if (error != String.Empty)
            {
                throw new ApplicationException("Error(s) loading: " + error);   
            }
        }

        protected override IJournalWriter CreateStoreSpecificJournalWriter(ulong lastEntryId)
        {
            return new StreamJournalWriter(this, _config);
        }

        protected override IEnumerable<Snapshot> LoadSnapshots()
        {
            var snapshots = new List<FileSnapshot>();
            foreach (var file in Directory.GetFiles(_config.Location.OfSnapshots, "*.snapshot"))
            {
                var fileInfo = new FileInfo(file);
                snapshots.Add(FileSnapshot.FromFileInfo(fileInfo));
            }

            snapshots.Sort((a, b) => a.LastEntryId.CompareTo(b.LastEntryId));
            return snapshots;
        }
    }
}
