using System;
using BlackBox;
using ILogFactory = LiveDomain.Core.Logging.ILogFactory;
using ILog = LiveDomain.Core.Logging.ILog;
using LogProvider = LiveDomain.Core.Logging.LogProvider;

namespace Livedomain.Modules.Blackbox
{
    public class BlackBoxLogFactory : ILogFactory
    {
        readonly LogKernel _kernel;

        /// <summary>
        /// Looks for blackbox config in application config file. 
        /// If missing, logs to a file in the current working directory or App_Data
        /// </summary>
        public BlackBoxLogFactory()
        {
            var config = LogConfiguration.FromConfigSection();
            
            if(config == null)
            {
                config = new LogConfiguration();
                var sink = new FileSink();
                sink.FileName = LogProvider.GetDefaultLogFile();
                config.Sinks.Add(sink);
            }

            _kernel = new LogKernel(config);

        }

        public ILog GetLog(Type type)
        {
            var logger = _kernel.GetLogger(type);
            return new BlackBoxLogAdapter(logger);
        }

        public ILog GetLogForCallingType()
        {
            var logger = _kernel.GetLogger();
            return new BlackBoxLogAdapter(logger);
        }
    }
}
