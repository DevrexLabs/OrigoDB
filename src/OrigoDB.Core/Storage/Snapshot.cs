using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public readonly long LastEntryId;


        public Snapshot(DateTime created, long lastEntryId)
        {
            Created = created;
            LastEntryId = lastEntryId;
        }
    }

    public class FileSnapshot : Snapshot
    {
        public string Name { get { return ToString(); } }


        public FileSnapshot(DateTime created, long lastEntryId) : base(created, lastEntryId)
        {
         
        }


        const string Pattern = @"^(?<lastEntryNr>\d{9}).snapshot$";
        private static Regex _parser = new Regex(Pattern);
        public static FileSnapshot FromFileInfo(FileInfo fileInfo)
        {
            Match m = _parser.Match(fileInfo.Name);
            if (!m.Success) throw new ArgumentException("Invalid snapshot filename");
            long entryNr = m.Groups["entryNr"].Value.ParsePadded();
            return new FileSnapshot(fileInfo.CreationTime, entryNr);
        }

        public override string ToString()
        {
            return String.Format("{0:000000000}.snapshot", LastEntryId);
        }
    }
}
