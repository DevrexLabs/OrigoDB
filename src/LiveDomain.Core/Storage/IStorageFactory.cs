using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core.Configuration;

namespace LiveDomain.Core.Storage
{
    public interface IStorageFactory
    {
        IStorage CreateStorage(EngineConfiguration config);
    }
}
