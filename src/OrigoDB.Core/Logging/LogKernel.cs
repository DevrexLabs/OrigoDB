using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Logging
{
    public sealed class LogKernel
    {

        readonly List<LogSink> _sinks;
        readonly Dictionary<string, Log> _logs;

        public bool SupressExceptions { get; set; }

        public LogKernel(LogConfiguration config = null)
        {
            SupressExceptions = true;
            _logs = new Dictionary<string, Log>(StringComparer.InvariantCultureIgnoreCase);
            _sinks = new List<LogSink>();
            if (config != null) Reconfigure(config);
        }

        public void Reconfigure(LogConfiguration config)
        {
            Ensure.NotNull(config, "config");
            lock (this)
            {
                _sinks.Clear();
                foreach (var sink in config.Sinks)
                {
                    _sinks.Add(sink);
                }
            }
        }

        internal void AddWriter(LogSink sink)
        {
            Ensure.NotNull(sink, "sink");
            lock(_sinks)
            {
                if (!_sinks.Contains(sink)) _sinks.Add(sink);    
            }
        }

        internal void RemoveWriter(LogSink sink)
        {
            Ensure.NotNull(sink, "sink");
            lock (_sinks)
            {
                _sinks.Remove(sink);    
            }
        }

        public LogKernel(LogSink writer)
            : this()
        {
            _sinks.Add(writer);
        }


        public Log LogFor(string name)
        {
            lock (_logs)
            {
                if (!_logs.ContainsKey(name))
                {
                    _logs[name] = new Log(this, name);
                }
                return _logs[name];
            }
        }


        internal void Dispatch(string logger, LogLevel logLevel, string messageTemplate, object[] args)
        {
            string message = null;
            lock (_sinks)
            {
                foreach (LogSink sink in _sinks)
                {
                    try
                    {
                        message = message ?? String.Format(messageTemplate, args);
                        sink.Write(logger, logLevel, message);
                    }
                    catch (Exception)
                    {
                      if(!SupressExceptions) throw;
                    }
                }
            }
        }
    }
}