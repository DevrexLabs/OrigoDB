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
    public sealed class FileCommandStore : CommandStore
    {
        private List<JournalFile> _journalFiles;

        public IEnumerable<JournalFile> JournalFiles
        {
            get
            {
                foreach (var journalFile in _journalFiles) yield return journalFile;
            }
        }

        public FileCommandStore(EngineConfiguration config) : base(config)
        {
        }


        /// <summary>
        /// Read journal files
        /// </summary>
        public override void Initialize()
        {
            EnsureDirectoryExists(_config.Location.OfJournal);
            base.Initialize();
            _journalFiles = new List<JournalFile>();
            foreach (var file in Directory.GetFiles(_config.Location.OfJournal, "*.journal"))
            {
                string fileName = new FileInfo(file).Name;
                _journalFiles.Add(JournalFile.Parse(fileName));
            }

            _journalFiles.Sort((a,b) => a.FileSequenceNumber.CompareTo(b.FileSequenceNumber));
        }


        public override IEnumerable<JournalEntry> GetJournalEntriesFrom(ulong entryId)
        {
            bool firstEntry = true;

            //Scroll to the correct file
            int offset = 0;
	        while (_journalFiles.Count > offset + 1 && _journalFiles[offset + 1].StartingEntryId < entryId)
				offset++;

            foreach (var journalFile in _journalFiles.Skip(offset))
            {
                string path = Path.Combine(_config.Location.OfJournal, journalFile.Name);
                using (Stream stream = GetReadStream(path))
                {
                    foreach (var entry in _formatter.ReadToEnd<JournalEntry>(stream))
                    {
                        if (firstEntry && entry.Id > entryId)
                        {
                            string msg = String.Format("requested: {0}, first: {1}", entryId, entry.Id);
                            throw new InvalidOperationException(msg);
                        }
                        firstEntry = false;
                        if (entry.Id < entryId) continue;
                        yield return entry;
                    }
                }
            }

        }

        public override IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime)
        {
            return GetJournalEntries()
                .Where(journalEntry => journalEntry.Created <= pointInTime);
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
            return new FileStream(path, FileMode.Append, FileAccess.Write);
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

        protected override IJournalWriter CreateStoreSpecificJournalWriter()
        {
            return new StreamJournalWriter(_config, CreateJournalWriterStream);
        }
    }
}
