using System;
using System.Collections.Generic;

namespace OrigoDB.Core
{
    /// <summary>
    /// Storage provider for the command journal
    /// </summary>
    public interface ICommandStore
    {
        /// <summary>
        /// Delete all of the commands in the store less than or including a given revision
        /// </summary>
        void Truncate(ulong revision);

        /// <summary>
        /// Connect and read meta data
        /// </summary>
        void Initialize();

        /// <summary>
        /// Retrieve all of the journal entries
        /// </summary>
        /// <returns></returns>
        IEnumerable<JournalEntry> GetJournalEntries();

        /// <summary>
        /// Retrieve journal entries with an Id >= the given entryId.
        /// </summary>
        IEnumerable<JournalEntry> GetJournalEntriesFrom(ulong id);

        /// <summary>
        /// Return journal entries at or before a specific point in time.
        /// </summary>
        /// <param name="pointInTime"></param>
        /// <returns></returns>
        IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime);

        /// <summary>
        /// Create a journal writer capable of writing Journal entries to the store
        /// </summary>
        /// <param name="nextRevision"></param>
        /// <returns></returns>
        IJournalWriter CreateJournalWriter(ulong nextRevision);

        /// <summary>
        /// No journal entries in the store
        /// </summary>
        bool IsEmpty
        {
            get;
        }

        /// <summary>
        /// Retrieve entries of type committed command, other entry types and rollbacked commands are filtered out.
        /// </summary>
        /// <param name="entryId">Start reading from this position</param>
        IEnumerable<JournalEntry<Command>> CommandEntriesFrom(ulong entryId);

        /// <summary>
        /// Retrieve entries of type committed command, other entry types and rollbacked commands are filtered out.
        /// </summary>
        IEnumerable<JournalEntry<Command>> CommandEntries();
    }

}