using System;
using System.Collections.Generic;
using System.Text;
using LiveDomain.Core;
using BlackBox;
using LiveDomain.Core.Logging;
using LogLevel = BlackBox.LogLevel;

namespace Livedomain.Modules.Blackbox
{
    class BlackBoxLogAdapter : ILog
    {
        private readonly ILogger _logger;

        public BlackBoxLogAdapter(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
        }

        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }
        
        public void Info(string message, params object[] args)
        {
            _logger.Information(message, args);
        }

        public void Warn(string message, params object[] args)
        {
            _logger.Warning(message, args);
        }

        public void Error(string message, params object[] args)
        {
            _logger.Error(message, args);
        }

        public void Exception(Exception exception)
        {
            _logger.Write(LogLevel.Error, exception);
        }

        public void Fatal(string message, params object[] args)
        {
            _logger.Write(LogLevel.Fatal, message, args);
        }
    }
}
