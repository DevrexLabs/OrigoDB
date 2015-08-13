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
            get { return _journalFiles; }
        }

        public FileCommandStore(EngineConfiguration config) : base(config)
        {
        }


        /// <summary>
        /// Read and cache journal file names
        /// </summary>
        public override void Initialize()
        {
            var journalPath = _config.GetJournalPath();
            EnsureDirectoryExists(journalPath);
            base.Initialize();
            _journalFiles = new List<JournalFile>();
            foreach (var file in Directory.GetFiles(journalPath, "*.journal"))
            {
                string fileName = new FileInfo(file).Name;
                _journalFiles.Add(JournalFile.Parse(fileName));
            }

            _journalFiles.Sort((a, b) => a.FileSequenceNumber.CompareTo(b.FileSequenceNumber));
        }


        protected override IEnumerable<JournalEntry> GetJournalEntriesFromImpl(ulong entryId)
        {
            //Scroll to the correct file
            int offset = 0;
            while (_journalFiles.Count > offset + 1 && _journalFiles[offset + 1].StartingEntryId < entryId)
                offset++;

            foreach (var journalFile in _journalFiles.Skip(offset))
            {
                string path = Path.Combine(_config.GetJournalPath(), journalFile.Name);
                using (var stream = GetReadStream(path))
                {
                    foreach (var entry in _formatter.ReadToEnd<JournalEntry>(stream).SkipWhile(e => e.Id < entryId))
                    {
                        yield return entry;
                    }
                }
            }

        }

        /// <summary>
        /// Create a new journal writer stream. The first entry written to the 
        /// stream will have the specified sequenceNumber
        /// </summary>
        /// <returns>A writeable stream</returns>
        public override Stream CreateJournalWriterStream(ulong firstEntryId = 1)
        {
            var current = _journalFiles.LastOrDefault() ?? new JournalFile(0, 0);
            var next = current.Successor(firstEntryId);
            _journalFiles.Add(next);
            string fileName = next.Name;
            string path = Path.Combine(_config.GetJournalPath(), fileName);
            return new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);
        }


        internal static void EnsureDirectoryExists(string directory)
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
    }
}
