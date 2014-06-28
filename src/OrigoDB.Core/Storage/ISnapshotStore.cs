using System.Collections.Generic;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core
{
    public interface ISnapshotStore
    {
        /// <summary>
        /// Snapshot metadata
        /// </summary>
        IEnumerable<Snapshot> Snapshots { get; }

        /// <summary>
        /// Create a snapshot of the provided model and save to storage
        /// </summary>
        void WriteSnapshot(Model mode);


        Model LoadSnapshot(Snapshot snapshot);

        /// <summary>
        /// Connect and read meta data
        /// </summary>
        void Initialize();

        /// <summary>
        /// No shapshots in the store 
        /// </summary>
        bool IsEmpty { get; }
    }
}