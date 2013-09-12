using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OrigoDB.Core
{

    /// <summary>
    /// Represents a journal file name. The name is composed of the 
    /// file sequence number and the id of the first journal entry in the file
    /// </summary>
    public class JournalFile
    {
        /// <summary>
        /// File name without path information
        /// </summary>
        public string Name
        {
            get { return ToString(); }
        }

        /// <summary>
        /// Id of the first journal entry in the file
        /// </summary>
        public readonly long StartingEntryId;


        /// <summary>
        /// sequence number of the file.
        /// </summary>
        public readonly long FileSequenceNumber;



        public JournalFile(long fileSequenceNumber, long startingEntryId )
        {
            FileSequenceNumber = fileSequenceNumber;
            StartingEntryId = startingEntryId;
        }

        static Regex journalFilenameParser = new Regex(@"^(?<fileNr>\d{9}).(?<entryNr>\d{9}).journal$");

        public static JournalFile Parse(string filename)
        {
            Match match = journalFilenameParser.Match(filename);
            if (!match.Success) throw new ArgumentException("bad journal filename format");

            long fileNr = match.Groups["fileNr"].Value.ParsePadded();
            long entryNr = match.Groups["entryNr"].Value.ParsePadded();
            return new JournalFile(fileNr, entryNr);
        }





        public override string ToString()
        {
            const string template = "{0:000000000}.{1:000000000}.journal";
            return String.Format(template, FileSequenceNumber, StartingEntryId);
        }


        public JournalFile Successor(long startingEntryId)
        {
            return new JournalFile(FileSequenceNumber +1, startingEntryId);
        }
    }
}
