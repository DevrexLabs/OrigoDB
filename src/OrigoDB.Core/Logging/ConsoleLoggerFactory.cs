using System;
using System.Diagnostics;

namespace OrigoDB.Core.Logging
{
    public class ConsoleLoggerFactory : ILoggerFactory
    {
        public ILogger GetLogger(Type type)
        {
            return new ConsoleLogger(type.FullName);
        }

        public ILogger GetLoggerForCallingType()
        {
            var type = new StackFrame(skipFrames: 1).GetMethod().DeclaringType;
            return GetLogger(type);
        }
    }
}