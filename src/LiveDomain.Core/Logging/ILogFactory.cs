using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Logging
{
    public interface ILogFactory
    {
        ILog GetLog(Type type);
        ILog GetLogForCallingType();
    }
}
