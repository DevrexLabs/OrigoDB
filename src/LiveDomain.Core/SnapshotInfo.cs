using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    [Serializable]
    public class SnapshotInfo
    {
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public long SizeOnDiscInBytes { get; set; }
    }
}
