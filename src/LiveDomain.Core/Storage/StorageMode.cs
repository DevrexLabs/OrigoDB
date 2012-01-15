using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    [Serializable]
    public enum StorageMode
    {
        FileSystem,
        None,
        SQL,
        Azure
    }
}
