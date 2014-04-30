using System;

namespace OrigoDB.Core.Storage
{

    public class Snapshot
    {

        /// <summary>
        /// Point in time when snapshot was taken
        /// </summary>
        public readonly DateTime Created;


        /// <summary>
        /// The id of the journal entry containing the last command applied to this snapshot
        /// </summary>
        public readonly ulong LastEntryId;


        public Snapshot(DateTime created, ulong lastEntryId)
        {
            Created = created;
            LastEntryId = lastEntryId;
        }
    }
}
