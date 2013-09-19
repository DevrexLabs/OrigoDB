using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OrigoDB.Core.Logging
{
    public class NullLoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// we can return the same instance to everyone
        /// </summary>
        private NullLogger _logger = new NullLogger();

        public ILogger GetLogger(Type type)
        {
            return _logger;
        }

        public ILogger GetLogger(string name)
        {
            return _logger;
        }

        public ILogger GetLoggerForCallingType()
        {
            return _logger;
        }
    }
}
