using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    internal class StorageSettings
    {
        public string CommandLogPath { get; set; }
        public string BaseImagePath { get; set; }
        public string SnapshotDirectory { get; set; }
    }
}
