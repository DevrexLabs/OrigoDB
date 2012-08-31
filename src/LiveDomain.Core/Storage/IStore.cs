using System;
using System.Collections.Generic;
using System.IO;

namespace LiveDomain.Core
{
    /// <summary>
    /// Responsible for persistent storage of the command journal and snapshots
    /// </summary>
    public interface IStore
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

        /// <summary>
        /// Checks the integrity of the configuration and throw if Create() will fail
        /// </summary>
        void VerifyCanCreate();

        /// <summary>
        /// Perform initial preparation of the storage
        /// </summary>
        /// <param name="model"></param>
        void Create(Model model);

        void Load();

        bool Exists { get; }

        IEnumerable<JournalEntry<Command>> GetJournalEntries();

        /// <summary>
        /// Retrieve journal entries with an Id >= the given entryId
        /// </summary>
        IEnumerable<JournalEntry<Command>> GetJournalEntriesFrom(long entryId);
        IEnumerable<JournalEntry<Command>> GetJournalEntriesFrom(DateTime pointInTime);
        IJournalWriter CreateJournalWriter(long lastEntryId);
    }
}