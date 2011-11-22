using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{



    public enum JournalWriterCreateOptions
    {
        Append,
        NextFragment
    }


    /// <summary>
    /// Responsible for reading/writing files and keeping track of Sequencing
    /// </summary>
	internal interface IPersistentStorage
	{

        IEnumerable<JournalEntry<Command>> GetJournalEntries(JournalFragmentInfo position);
        

        /// <summary>
        /// Should verify the integrity of an existing database and throw unless the state is valid
        /// </summary>
        void VerifyCanLoad();

        /// <summary>
        /// Should return null if no snapshot exists. Used as base image when 
        /// restoring to latest state.
        /// </summary>
        /// <param name="state">A reference to the first fragment in the sequence following the returned snapshot</param>
        /// <returns></returns>
        Model GetMostRecentSnapshot(out JournalFragmentInfo journalState);

        /// <summary>
        /// Write a snapshot to disk
        /// </summary>
        /// <param name="model"></param>
        /// <param name="name"></param>
        void WriteSnapshot(Model model, string name);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        Stream CreateJournalWriterStream(JournalWriterCreateOptions options);

        void VerifyCanCreate();

        void Create(Model model);
    }
}
