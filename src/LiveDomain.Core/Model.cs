using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    /// <summary>
    /// Derive your model from this class, mark it serializable.
    /// </summary>
    [Serializable]
    public abstract class Model
    {

        /// <summary>
        /// Override this method to perform custom initialization after the model has been materialized.
        /// </summary>
        protected internal virtual void OnLoad() { }

    }
}
