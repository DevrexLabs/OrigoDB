using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace LiveDomain.Core.Storage
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
        /// The sequence number of the last command executed
        /// </summary>
        public readonly long LastSequenceNumber;


        public Snapshot(DateTime created, long lastSequenceNumber)
        {
            Created = created;
            LastSequenceNumber = lastSequenceNumber;
        }
    }

    public class FileSnapshot : Snapshot
    {
        public string Name { get { return ToString(); } }


        public FileSnapshot(DateTime created, long lastSequenceNumber) : base(created, lastSequenceNumber)
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
            return String.Format("{0:000000000}.snapshot", LastSequenceNumber);
        }
    }
}
