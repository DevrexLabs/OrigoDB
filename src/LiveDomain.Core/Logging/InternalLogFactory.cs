using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LiveDomain.Core.Logging
{
    public class InternalLogFactory : ILogFactory
    {
        readonly Func<String, ILog> _creator;
        readonly Dictionary<String, ILog> _logs; 

        public InternalLogFactory() : this(name => new FileLogger(name))
        {
        }

        public InternalLogFactory(Func<String, ILog> logCreator)
        {
            if(logCreator == null) throw new ArgumentNullException("logCreator");
            _creator = logCreator;
            _logs = new Dictionary<string, ILog>(StringComparer.InvariantCultureIgnoreCase);
        }

        public ILog GetLog(Type type)
        {
            return GetLog(type.FullName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ILog GetLogForCallingType()
        {
            var frame = new StackFrame(1, false);
            var typeOfCaller = frame.GetMethod().DeclaringType;
            return GetLog(typeOfCaller);

        }

        public ILog GetLog(string name)
        {
            if(_logs.ContainsKey(name) == false)
            {
                _logs[name] = _creator.Invoke(name);
            }
            return _logs[name];
        }
    }
}
