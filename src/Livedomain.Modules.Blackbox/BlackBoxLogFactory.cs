using System;
using BlackBox;
using LiveDomain.Core.Logging;

namespace Livedomain.Modules.Blackbox
{
    public class BlackBoxLogFactory : ILogFactory
    {
        readonly LogKernel _kernel;

        public BlackBoxLogFactory()
        {
            var config = LogConfiguration.FromConfigSection();
            
            if(config == null)
            {
                config = new LogConfiguration();
                var sink = new ConsoleSink();
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

        public ILog GetLog(string name)
        {
            throw new NotImplementedException();
        }
    }
}
