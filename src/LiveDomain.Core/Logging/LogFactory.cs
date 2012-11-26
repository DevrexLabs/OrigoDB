using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace LiveDomain.Core.Logging
{
    public class LogFactory : ILogFactory
    {
        public readonly LogKernel Kernel;

        public LogFactory()
        {
            LogConfiguration config = LogConfiguration.FromConfigFile();
            if (config == null)
            {
                config = new LogConfiguration();
                config.Sinks.Add(new ConsoleSink());
                config.Sinks.Add(new MemorySink());
            }
            Kernel = new LogKernel(config);
        }

        public ILog GetLog(Type type)
        {
            return Kernel.LogFor(type.FullName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ILog GetLogForCallingType()
        {
            var frame = new StackFrame(1, false);
            var typeOfCaller = frame.GetMethod().DeclaringType;
            return GetLog(typeOfCaller);

        }
    }
}
