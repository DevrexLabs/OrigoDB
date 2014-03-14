using System;
using System.Text.RegularExpressions;
using System.IO;

namespace OrigoDB.Core.Storage
{

    /// <summary>
    /// 
    /// </summary>
    public class Snapshot
    {

        /// <summary>
        /// Point in time when snapshot was taken
        /// </summary>
        public readonly DateTime Created;


        /// <summary>
        /// The id of the journal entry containing the last command applied to this snapshot
        /// </summary>
        public readonly ulong LastEntryId;


        public Snapshot(DateTime created, ulong lastEntryId)
        {
            Created = created;
            LastEntryId = lastEntryId;
        }
    }

    public class FileSnapshot : Snapshot
    {
        public string Name { get { return ToString(); } }


        public FileSnapshot(DateTime created, ulong lastEntryId) : base(created, lastEntryId)
        {
         
        }


        const string Pattern = @"^(?<lastEntryNr>\d{9}).snapshot$";
        private static Regex _parser = new Regex(Pattern);
        public static FileSnapshot FromFileInfo(FileInfo fileInfo)
        {
            Match m = _parser.Match(fileInfo.Name);
            if (!m.Success) throw new ArgumentException("Invalid snapshot filename");
            ulong entryNr = m.Groups["entryNr"].Value.ParsePadded();
            return new FileSnapshot(fileInfo.CreationTime, entryNr);
        }

        public override string ToString()
        {
            return String.Format("{0:000000000}.snapshot", LastEntryId);
        }
    }
}
