using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Logging
{
    /// <summary>
    /// Returns the same ILog instance for calls to any GetLog() methods
    /// </summary>
    public class SingletonLogFactory : ILogFactory
    {
        private readonly ILog _instance;

        public SingletonLogFactory(ILog instance)
        {
            _instance = instance;
        }
        public ILog GetLog(Type type)
        {
            return _instance;
        }

        public ILog GetLogForCallingType()
        {
            return _instance;
        }

        public ILog GetLog(string name)
        {
            return _instance;
        }
    }
}
