using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{
    public interface ICommandJournal : IDisposable
	{
        /// <summary>
        /// Iterator over the entire sequence of journalentries in order.
        /// </summary>
        /// <returns></returns>
        IEnumerable<JournalEntry<Command>> GetAllEntries();


	    /// <summary>
        /// Returns journal entries in order created on or after the specified point in time
        /// </summary>
        IEnumerable<JournalEntry<Command>> GetEntriesFrom(DateTime pointInTime);

        /// <summary>
        /// Returns an iterator over the journal entires in order starting with a specific sequence number
        /// </summary>
	    IEnumerable<JournalEntry<Command>> GetEntriesFrom(long sequenceNumber);


        /// <summary>
        /// Write a command to the log
        /// </summary>
        void Append(Command command);

        /// <summary>
        /// Id of the last entry written to the log
        /// </summary>
        long LastEntryId { get; }

        /// <summary>
        /// Mark the previous transaction as failed
        /// </summary>
        void WriteRollbackMarker();
    }
}
