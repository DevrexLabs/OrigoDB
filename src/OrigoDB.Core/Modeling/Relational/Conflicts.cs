using System;
using System.Collections.Generic;

namespace OrigoDB.Core.Modeling.Relational
{
    [Serializable]
    public class Conflicts
    {
        /// <summary>
        /// Keys that already existed
        /// </summary>
        public readonly List<EntityKey> Inserts = new List<EntityKey>();

        /// <summary>
        /// Didn't exist or version didn't match, returned version reflects which case
        /// </summary>
        public readonly List<EntityKey> Updates = new List<EntityKey>();

        /// <summary>
        /// Didn't exist or Version didn't match, returned Version reflects which case.
        /// </summary>
        public readonly List<EntityKey> Deletes = new List<EntityKey>(); 
    }
}