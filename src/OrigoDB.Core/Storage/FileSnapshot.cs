using System;
using System.Text.RegularExpressions;

namespace OrigoDB.Core.Storage
{
    public class FileSnapshot : Snapshot
    {
        public string Name { get { return ToString(); } }


        public FileSnapshot(DateTime created, ulong lastEntryId) : base(created, lastEntryId)
        {
         
        }

        const string Pattern = @"^(?<entryNr>\d{9}).snapshot$";

        readonly private static Regex _parser = new Regex(Pattern);

        public static FileSnapshot FromFileInfo(string fileName, DateTime created)
        {
            Match m = _parser.Match(fileName);
            if (!m.Success) throw new ArgumentException("Invalid snapshot filename");
            ulong entryNr = m.Groups["entryNr"].Value.ParsePadded();
            return new FileSnapshot(created, entryNr);
        }

        public override string ToString()
        {
            return String.Format("{0:000000000}.snapshot", Revision);
        }
    }
}