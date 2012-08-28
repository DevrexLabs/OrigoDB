using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Storage
{
    public interface IStorageFactory
    {
        IStorage CreateStorage(EngineConfiguration config);
    }
}
