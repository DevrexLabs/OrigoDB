using System;

namespace OrigoDB.Core.Storage
{
    /// <summary>
    /// Snapshot metadata
    /// </summary>
    public class Snapshot
    {

        /// <summary>
        /// Point in time when snapshot was taken
        /// </summary>
        public readonly DateTime Created;


        /// <summary>
        /// The Revision of the Model
        /// </summary>
        public readonly ulong Revision;


        public Snapshot(DateTime created, ulong revision)
        {
            Created = created;
            Revision = revision;
        }
    }
}
