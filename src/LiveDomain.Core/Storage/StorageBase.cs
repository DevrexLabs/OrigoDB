using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using LiveDomain.Core.Configuration;

namespace LiveDomain.Core.Storage
{



    /// <summary>
    /// Responsible for naming scheme and file ordering
    /// </summary>
    abstract class StorageBase : IStorage
    {

        EngineConfiguration _config;
        ISerializer _serializer;

        protected EngineConfiguration EngineConfiguration
        {
            get { return _config; }
        }

        protected StorageBase(EngineConfiguration config)
        {
            _config = config;
            _serializer = _config.CreateSerializer();
        }



        #region Abstract members
        /// <summary>
        /// Retrieve names of all the items, both journal pieces and snapshots
        /// </summary>
        protected abstract IEnumerable<string> GetItemIdentifiers();

        /// <summary>
        /// Remove the item from storage
        /// </summary>
        /// <param name="name"></param>
        protected abstract void RemoveSnapshot(string id);
        protected abstract Stream GetReadStream(string id);
        protected abstract bool Exists(string id);
        protected abstract Stream GetWriteStream(string id, bool append);
        public abstract void VerifyCanCreate();
        public abstract void VerifyCanLoad();
        public abstract void Initialize();

        #endregion
        
        private IEnumerable<StorageBlobIdentifier> GetJournalItems()
        {
            return GetItemIdentifiers()
                .Select(id => new StorageBlobIdentifier(id))
                .Where(item => item.IsJournalSegment);
        }

        /// <summary>
        /// Journal must be closed by caller before writing snapshot because it will we rolling over
        /// </summary>
        /// <param name="model"></param>
        /// <param name="name"></param>
        public void WriteSnapshot(Model model, string name)
        {
            if (name == null) name = String.Empty;
            const string namePattern = "^[-a-z0-9_.]*$";
            
            if (!Regex.IsMatch(name, namePattern, RegexOptions.IgnoreCase))
            {
                throw new ArgumentException("Snapshot name must satisfy the pattern '^[-a-z0-9_.]*$'", "name");
            }

            int currentJournalSequenceNumber = GetJournalItems().Last().SequenceNumber;
            StorageBlobIdentifier id = StorageBlobIdentifier.CreateSnapshot(currentJournalSequenceNumber, name);

            using (Stream stream = GetWriteStream(id.ToString(), false))
            {
                _serializer.Write(model, stream);
            }
        }

        public IEnumerable<JournalEntry<Command>> GetJournalEntries(JournalSegmentInfo segment)
        {
            var sequence = GetJournalItems()
                .SkipWhile(item => item.SequenceNumber < segment.SequenceNumber)
                .ToArray();

            foreach (StorageBlobIdentifier item in sequence)
            {
                using (Stream stream = GetReadStream(item.ToString()))
                {
                    foreach (var entry in _serializer.ReadToEnd<JournalEntry<Command>>(stream))
                    {
                        yield return entry;
                    }
                }
            }
        }

        public Model GetMostRecentSnapshot(out JournalSegmentInfo segment)
        {
            StorageBlobIdentifier snapshotId = GetItemIdentifiers()
                    .Select(name => new StorageBlobIdentifier(name))
                    .Where(id => id.IsSnapshot)
                    .OrderByDescending(id => id.SequenceNumber)
                    .FirstOrDefault();

            if (snapshotId == null)
            {
                segment = new JournalSegmentInfo(1);
                return null;
            }

            segment = new JournalSegmentInfo(snapshotId.SequenceNumber + 1);

            using (Stream stream = GetReadStream(snapshotId.ToString()))
            {
                return _serializer.Read<Model>(stream);
            }
        }

        public Stream CreateJournalWriterStream(JournalWriterCreateOptions options)
        {
            StorageBlobIdentifier lastSegment = GetJournalItems().LastOrDefault();

            if (lastSegment == null)
                lastSegment = StorageBlobIdentifier.CreateJournal(1);

            if (options == JournalWriterCreateOptions.NextSegment)
                lastSegment = lastSegment.Successor();

            bool append = options == JournalWriterCreateOptions.Append;
            return GetWriteStream(lastSegment.ToString(), append);
        }

        public void Create(Model initialModel)
        {
            VerifyCanCreate();
            Initialize();

            string id = StorageBlobIdentifier.InitialSnapshot.ToString();
            using (Stream stream = GetWriteStream(id, false))
            {
                _serializer.Write(initialModel, stream);
            }

            CreateJournalWriterStream(JournalWriterCreateOptions.Append).Close();
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
    }
}
