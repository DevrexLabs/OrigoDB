using System;
using BlackBox;
using OrigoDB.Core.Utilities;
namespace OrigoDB.Modules.Blackbox
{
    class BlackBoxLogAdapter : Core.Logging.ILogger
    {
        private readonly ILogger _logger;

        public BlackBoxLogAdapter(ILogger logger)
        {
            Ensure.NotNull(logger, "logger");
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

        public void Trace(string messageTemplate, params object[] args)
        {
            _logger.Write(LogLevel.Verbose, messageTemplate, args);
        }

        public void Trace(Exception exception)
        {
            _logger.Write(LogLevel.Verbose, exception);
        }

        public void Trace(Func<string> messageGenerator)
        {
            _logger.Verbose(messageGenerator.Invoke());
        }


        public void Debug(Exception exception)
        {
            _logger.Write(LogLevel.Debug, exception);
        }

        public void Debug(Func<string> messageGenerator)
        {
            _logger.Debug(messageGenerator.Invoke());
        }

        public void Info(Exception exception)
        {
            _logger.Write(LogLevel.Information, exception);
        }

        public void Info(Func<string> messageGenerator)
        {
            _logger.Information(messageGenerator.Invoke());
        }

        public void Warn(Exception exception)
        {
            _logger.Write(LogLevel.Warning, exception);
        }

        public void Warn(Func<string> messageGenerator)
        {
            _logger.Warning(messageGenerator.Invoke());
        }

        public void Error(Exception exception)
        {
            _logger.Write(LogLevel.Error, exception);
        }

        public void Error(Func<string> messageGenerator)
        {
            _logger.Error(messageGenerator.Invoke());
        }

        public void Fatal(Exception exception)
        {
            _logger.Write(LogLevel.Fatal, exception);
        }

        public void Fatal(Func<string> messageGenerator)
        {
            _logger.Fatal(messageGenerator.Invoke());
        }

        public void Fatal(string message, params object[] args)
        {
           _logger.Fatal(message,args); 
        }
    }
}
