using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LiveDomain.Core
{
    /// <summary>
    /// StorageBlobIdentifier maintains the naming scheme of storage items
    /// <remarks>
    /// A snapshot is a projection of the commands contained in the journal sequence 
    /// including the journal segment with the same sequence number.
    /// </remarks>
    /// </summary>
    internal class StorageBlobIdentifier
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

        public static readonly StorageBlobIdentifier InitialSnapshot 
            = new StorageBlobIdentifier(0, SnapshotSuffix, String.Empty);

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


        private StorageBlobIdentifier(int sequenceNumber, string suffix, string name)
        {
            SequenceNumber = sequenceNumber;
            Suffix = suffix;
            Name = name;
            string namePart = String.Empty;
            if (name != String.Empty) namePart = name + ".";
            _id = String.Format("{0:000000000}.{1}{2}", SequenceNumber, namePart, Suffix);
        }

        public StorageBlobIdentifier Successor()
        {
            return new StorageBlobIdentifier(SequenceNumber + 1, Suffix, Name);
        }

        public static StorageBlobIdentifier CreateJournal(int sequenceNumber)
        {
            return new StorageBlobIdentifier(sequenceNumber, JournalSuffix, string.Empty);
        }


        static Regex _matcher = new Regex(ItemIdPattern, RegexOptions.IgnoreCase);

        public bool IsJournalSegment 
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

        public StorageBlobIdentifier(string id)
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

        public static StorageBlobIdentifier CreateSnapshot(int sequenceNumber, string name)
        {
            return new StorageBlobIdentifier(sequenceNumber, SnapshotSuffix, name);
        }
    }
}
