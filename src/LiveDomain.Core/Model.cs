using System;
using LiveDomain.Core.Security;

namespace LiveDomain.Core
{
    /// <summary>
    /// Derive your model from this class, mark it serializable.
    /// </summary>
    [Serializable]
    public abstract class Model
    {

        internal IAuthenticator Authenticator { get; set; }
        
        internal IAuthorizer<Type> Authorizer { get; set; }

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
