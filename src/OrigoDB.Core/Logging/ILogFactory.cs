using System;

namespace OrigoDB.Core.Logging
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(Type type);
        ILogger GetLoggerForCallingType();
    }
}
