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
        NextSegment
    }


    /// <summary>
    /// Responsible for reading/writing files and keeping track of Sequencing
    /// </summary>
	public interface IStorage
	{

        IEnumerable<JournalEntry<Command>> GetJournalEntries(JournalSegmentInfo position);
        

        /// <summary>
        /// Should verify the integrity of an existing database and throw unless the state is valid
        /// </summary>
        void VerifyCanLoad();

        /// <summary>
        /// Should return null if no snapshot exists. Used as base image when 
        /// restoring to latest state.
        /// </summary>
        /// <param name="state">A reference to the first segment in the sequence following the returned snapshot</param>
        /// <returns></returns>
        Model GetMostRecentSnapshot(out JournalSegmentInfo journalState);

        /// <summary>
        /// Write a snapshot to disk
        /// </summary>
        /// <param name="model"></param>
        /// <param name="name"></param>
        void WriteSnapshot(Model model, string name);

        Stream CreateJournalWriterStream(JournalWriterCreateOptions options);

        /// <summary>
        /// Checks the integrity of the configuration and throw if Create() will fail
        /// </summary>
        void VerifyCanCreate();

        /// <summary>
        /// Perform initial preparation of the storage
        /// </summary>
        /// <param name="model"></param>
        void Create(Model model);

        bool Exists { get; }

        bool CanCreate { get; }
    }
}
