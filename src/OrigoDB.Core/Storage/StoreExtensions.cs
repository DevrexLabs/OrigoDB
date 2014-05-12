using System;
using System.Collections.Generic;

namespace OrigoDB.Core
{
    public static class StoreExtensions
    {

        /// <summary>
        /// Get commands beginning from a specific entry id (inclusive)
        /// </summary>
        public static IEnumerable<JournalEntry<Command>> CommandEntriesFrom(this IStore store, ulong entryId)
        {
            return CommittedCommandEntries(() => store.GetJournalEntriesFrom(entryId));
        }

        /// <summary>
        /// Get non rolled back commands from a point in time
        /// </summary>
        public static IEnumerable<JournalEntry<Command>> CommandEntriesFrom(this IStore store, DateTime pointInTime)
        {
            return CommittedCommandEntries(() => store.GetJournalEntriesBeforeOrAt(pointInTime));
        }

        /// <summary>
        /// Select the items of type Command that are not followed by a rollback marker
        /// </summary>
        public static IEnumerable<JournalEntry<Command>> CommittedCommandEntries(Func<IEnumerable<JournalEntry>> enumerator)
        {

            //rollback markers repeat the id of the rolled back command
            JournalEntry<Command> previous = null;

            foreach (JournalEntry current in enumerator.Invoke())
            {
                if (current is JournalEntry<Command>)
                {
                    (current as JournalEntry<Command>).Item.Timestamp = current.Created;
                    if (previous != null) yield return previous;
                    previous = (JournalEntry<Command>) current;
                }
                else previous = null;
            }
            if (previous != null) yield return previous;
        }

        /// <summary>
        /// Get the complete sequence of commands starting, excluding any that were rolled back
        /// </summary>
        public static IEnumerable<JournalEntry<Command>> CommandEntries(this IStore store)
        {
            return store.CommandEntriesFrom(1);
        }
    }
}