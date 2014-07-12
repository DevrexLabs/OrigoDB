using System;
using System.Text.RegularExpressions;

namespace OrigoDB.Core.Storage
{
    public class FileSnapshot : Snapshot
    {
        public string Name { get { return ToString(); } }


        public FileSnapshot(DateTime created, ulong lastEntryId)
            : base(created, lastEntryId)
        {

        }

        const string Pattern = @"^(?<entryNr>\d{9}).snapshot$";

        readonly private static Regex _parser = new Regex(Pattern);

        public static FileSnapshot FromFileInfo(string fileName, DateTime created)
        {
            Match m = _parser.Match(fileName);
            if (!m.Success) throw new ArgumentException("Invalid snapshot filename");
            ulong entryNr = ParsePadded(m.Groups["entryNr"].Value);
            return new FileSnapshot(created, entryNr);
        }

        public override string ToString()
        {
            return String.Format("{0:000000000}.snapshot", Revision);
        }

        public static UInt64 ParsePadded(string number)
        {
            //Get rid of the leading zeros
            number = number.TrimStart('0');
            if (number == "") return 0;
            return UInt64.Parse(number);
        }
    }
}