using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Configuration;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core
{
    /// <summary>
    /// Responsible for persistent storage of the command journal and snapshots
    /// </summary>
    public interface IStore
    {


        IEnumerable<Snapshot> Snapshots { get; }

        /// <summary>
        /// Connect and read meta data
        /// </summary>
        void Init();


        /// <summary>
        /// Load model and enter state accepting new commands to be written to the journal
        /// </summary>
        /// <returns></returns>
        JournalAppender LoadModel(out Model model, Type modelType = null);

        //JournalAppender GetAppender();
        
        //Model LoadMostRecentSnapshot(out ulong lastEntryId);

        /// <summary>
        /// Create a snapshot of the provided model and save to storage
        /// </summary>
        void WriteSnapshot(Model model);

        IEnumerable<JournalEntry> GetJournalEntries();

        /// <summary>
        /// Retrieve journal entries with an Id >= the given entryId. Used when restoring a snapshot
        /// </summary>
        IEnumerable<JournalEntry> GetJournalEntriesFrom(ulong entryId);

        /// <summary>
        /// Return journal entries at or before a specific point in time. Used for point in time recovert
        /// </summary>
        /// <param name="pointInTime"></param>
        /// <returns></returns>
        IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime);

        IJournalWriter CreateJournalWriter(ulong lastEntryId);


        Stream CreateJournalWriterStream(ulong firstEntryId = 1);


        /// <summary>
        /// Create from a specific instance by writing an initial snapshot
        /// </summary>
        /// <param name="model"></param>
        void Create(Model model);

        /// <summary>
        /// Create without a snapshot
        /// </summary>
        void Create(Type modelType);


        void Create<T>() where T : Model, new();


        bool IsEmpty
        {
            get;
        }
    }

}