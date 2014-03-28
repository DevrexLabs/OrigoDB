using System;

namespace OrigoDB.Core
{
    /// <summary>
    /// Derive your model from this class, mark it serializable.
    /// </summary>
    [Serializable]
    public abstract class Model : MarshalByRefObject
    {

        /// <summary>
        /// SnapshotRestored is called after the most recent snaphot has been loaded 
        /// but before any commands are restored.
        /// </summary>
        protected internal virtual void SnapshotRestored() { }

        /// <summary>
        /// This method is called after the model has been restored from 
        /// persistent storage and before the engine is available for transactions.
        /// </summary>
        protected internal virtual void JournalRestored() { }

    }
}
