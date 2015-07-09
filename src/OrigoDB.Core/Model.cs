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
        /// Version of the model, incremented by the Engine
        /// for each command executed.
        /// </summary>
        public ulong Revision
        {
            get;
            internal protected set;
        }

        protected Model(ulong revision)
        {
            Revision = revision;
        }

        protected Model(){}

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

        /// <summary>
        /// Called after model is loaded or created but before engine starts accepting requests 
        /// </summary>
        /// <param name="engine">The engine which loaded or created the model.</param>
        protected internal virtual void Starting(Engine engine)
        { }
    }
}
