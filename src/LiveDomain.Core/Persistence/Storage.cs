using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace LiveDomain.Core
{


    /// <summary>
    /// StorageItemIdentifier Responsible for naming scheme of storage items
    /// <remarks>
    /// A snapshot is a projection of the commands contained in the journal sequence 
    /// including the journal fragment with the same sequence number.
    /// </remarks>
    /// </summary>
    internal class StorageFragmentIdentifier
    {


        /// 000000001.journal
        /// 000000002.journal
        /// 000000003.journal
        /// 000000003.snapshot
        /// 000000004.journal
        /// 000000005.journal
        /// 000000006.name.snapshot
        /// 000000006.journal

        public const string JournalSuffix = "journal";
        public const string SnapshotSuffix = "snapshot";

        public static readonly StorageFragmentIdentifier InitialSnapshot 
            = new StorageFragmentIdentifier(0, SnapshotSuffix, String.Empty);

        public const string ItemIdPattern = @"^(?<seq>\d{9})\.((?<name>[-a-z0-9_.]+)\.)?(?<suffix>journal|snapshot)$";

        public readonly int SequenceNumber;

        /// <summary>
        /// The item suffix without a trailing dot, either journal or snapshot
        /// </summary>
        public readonly string Suffix;

        /// <summary>
        /// An optional name between the sequence and the suffix
        /// </summary>
        public readonly string Name;


        /// <summary>
        /// The entire id 
        /// </summary>
        string _id;


        private StorageFragmentIdentifier(int sequenceNumber, string suffix, string name)
        {
            SequenceNumber = sequenceNumber;
            Suffix = suffix;
            Name = name;
            string namePart = String.Empty;
            if (name != String.Empty) namePart = name + ".";
            _id = String.Format("{0:000000000}.{1}{2}", SequenceNumber, namePart, Suffix);
        }

        public StorageFragmentIdentifier Successor()
        {
            return new StorageFragmentIdentifier(SequenceNumber + 1, Suffix, Name);
        }

        public static StorageFragmentIdentifier CreateJournal(int sequenceNumber)
        {
            return new StorageFragmentIdentifier(sequenceNumber, JournalSuffix, string.Empty);
        }


        static Regex _matcher = new Regex(ItemIdPattern, RegexOptions.IgnoreCase);

        public bool IsJournalFragment 
        {
            get
            {
                return Suffix == JournalSuffix;
            }
        }

        public bool IsSnapshot
        {
            get
            {
                return Suffix == SnapshotSuffix;
            }
        }

        public StorageFragmentIdentifier(string id)
        {
            _id = id;

            if (id == null) throw new ArgumentNullException();
            Match match = _matcher.Match(id);
            if (!match.Success) throw new ArgumentException();
            
            //Extract sequence number
            string sequence = match.Groups["seq"].Value;

            //Get rid of the leading zeros
            sequence = sequence.TrimStart('0');

            //Fix for initial snapshot which has sequence number 0
            if (sequence == String.Empty) sequence = "0";

            //Pray that this won't throw
            SequenceNumber = Int32.Parse(sequence);

            //Extract suffix
            Suffix = match.Groups["suffix"].Value;

            //Extract optional name
            if (match.Groups["name"].Success) Name = match.Groups["name"].Value;
            else Name = String.Empty;

        }

        public override string ToString()
        {
            return _id;
        }

        public static StorageFragmentIdentifier CreateSnapshot(int sequenceNumber, string name)
        {
            return new StorageFragmentIdentifier(sequenceNumber, SnapshotSuffix, name);
        }
    }

    internal class JournalFragmentInfo
    {
        public readonly int SequenceNumber;
        public static readonly JournalFragmentInfo Initial = new JournalFragmentInfo(1);

        public JournalFragmentInfo(int sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }
    }


    /// <summary>
    /// Responsible for naming scheme and file ordering
    /// </summary>
    abstract class Storage : IPersistentStorage
    {

        EngineConfiguration _config;
        ISerializer _serializer;

        public Storage(EngineConfiguration config)
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
        
        private IEnumerable<StorageFragmentIdentifier> GetJournalItems()
        {
            return GetItemIdentifiers()
                .Select(id => new StorageFragmentIdentifier(id))
                .Where(item => item.IsJournalFragment);
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
            StorageFragmentIdentifier id = StorageFragmentIdentifier.CreateSnapshot(currentJournalSequenceNumber, name);

            using (Stream stream = GetWriteStream(id.ToString(), false))
            {
                _serializer.Write(model, stream);
            }
        }

        public IEnumerable<JournalEntry<Command>> GetJournalEntries(JournalFragmentInfo fragment)
        {
            var sequence = GetJournalItems()
                .SkipWhile(item => item.SequenceNumber < fragment.SequenceNumber)
                .ToArray();

            foreach (StorageFragmentIdentifier item in sequence)
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

        public Model GetMostRecentSnapshot(out JournalFragmentInfo fragment)
        {
            StorageFragmentIdentifier snapshotId = GetItemIdentifiers()
                    .Select(name => new StorageFragmentIdentifier(name))
                    .Where(id => id.IsSnapshot)
                    .OrderByDescending(id => id.SequenceNumber)
                    .FirstOrDefault();

            if (snapshotId == null)
            {
                fragment = new JournalFragmentInfo(1);
                return null;
            }

            fragment = new JournalFragmentInfo(snapshotId.SequenceNumber + 1);

            using (Stream stream = GetReadStream(snapshotId.ToString()))
            {
                return _serializer.Read<Model>(stream);
            }
        }

        public Stream CreateJournalWriterStream(JournalWriterCreateOptions options)
        {
            StorageFragmentIdentifier lastFragment = GetJournalItems().LastOrDefault();

            if (lastFragment == null)
                lastFragment = StorageFragmentIdentifier.CreateJournal(1);

            if (options == JournalWriterCreateOptions.NextFragment)
                lastFragment = lastFragment.Successor();

            bool append = options == JournalWriterCreateOptions.Append;
            return GetWriteStream(lastFragment.ToString(), append);
        }

        public void Create(Model initialModel)
        {
            VerifyCanCreate();
            Initialize();

            string id = StorageFragmentIdentifier.InitialSnapshot.ToString();
            using (Stream stream = GetWriteStream(id, false))
            {
                _serializer.Write(initialModel, stream);
            }

            CreateJournalWriterStream(JournalWriterCreateOptions.Append).Close();
        }
    }
}
