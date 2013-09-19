using System;
using BlackBox;
using ILogFactory = OrigoDB.Core.Logging.ILoggerFactory;
using ILog = OrigoDB.Core.Logging.ILogger;

namespace OrigoDB.Modules.Blackbox
{
    public class BlackBoxLogFactory : ILogFactory
    {
        readonly LogKernel _kernel;

        /// <summary>
        /// Looks for blackbox config in application config file. 
        /// If missing, logs to a file in the current working directory or App_Data
        /// </summary>
        public BlackBoxLogFactory(LogConfiguration config = null)
        {
            config = config ?? LogConfiguration.FromConfigSection() ?? new LogConfiguration();
            _kernel = new LogKernel(config);
        }

        public ILog GetLogger(Type type)
        {
            var logger = _kernel.GetLogger(type);
            return new BlackBoxLogAdapter(logger);
        }

        public ILog GetLoggerForCallingType()
        {
            var logger = _kernel.GetLogger();
            return new BlackBoxLogAdapter(logger);
        }
    }
}
