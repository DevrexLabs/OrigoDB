using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Journaling
{

    /// <summary>
    /// Journal entry following a failed command
    /// </summary>
    [Serializable]
    public class RollbackMarker
    {
        private RollbackMarker()
        {
        }

        public static RollbackMarker Instance = new RollbackMarker();
    }
}
