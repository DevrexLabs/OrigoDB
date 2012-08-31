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
    /// Responsible for persistent storage of the command journal and snapshots
    /// </summary>
	public interface IStorage
	{
        

        /// <summary>
        /// Should verify the integrity of an existing database and throw unless the state is valid
        /// </summary>
        void VerifyCanLoad();


        
        Model LoadMostRecentSnapshot(out long lastEntryId);

        /// <summary>
        /// Create a snapshot of the provided model and save to storage
        /// </summary>
        void WriteSnapshot(Model model, long lastEntryId);

        Stream CreateJournalWriterStream(long firstEntryId);


        //IJournalWriter CreateJournalWriter();

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

        IEnumerable<JournalEntry<Command>> GetJournalEntries();

        /// <summary>
        /// Retrieve journal entries with an Id >= the given entryId
        /// </summary>
        IEnumerable<JournalEntry<Command>> GetJournalEntriesFrom(long entryId);
        IEnumerable<JournalEntry<Command>> GetJournalEntriesFrom(DateTime pointInTime);

        IJournalWriter CreateJournalWriter(long lastEntryId);

        void Load();
    }
}
