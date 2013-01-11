using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Logging
{
    public class LogConfiguration
    {
        public readonly List<LogSink> Sinks;

        public LogConfiguration()
        {
            Sinks = new List<LogSink>();
        }

        public static LogConfiguration FromConfigFile()
        {
            return null;
        }

        
    }
}
