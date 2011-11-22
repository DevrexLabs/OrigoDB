using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    public enum SnapshotBehavior
    {

        /// <summary>
        /// No automatic snapshots
        /// </summary>
        None,

        /// <summary>
        /// Take a snapshot when the server starts up
        /// </summary>
        AfterRestore,

        /// <summary>
        /// Take a snaphot when the engine is shutting down
        /// </summary>
        OnShutdown
    }
}
