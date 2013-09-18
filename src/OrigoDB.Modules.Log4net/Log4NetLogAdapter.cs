using System;
using log4net;
using OrigoDB.Core.Logging;

namespace OrigoDB.Modules.Log4Net
{
	public class Log4NetLogAdapter : ILogger
	{
		readonly ILog _log;

		public Log4NetLogAdapter(ILog log)
		{
			_log = log;
		}

		public void Debug(string message, params object[] args)
		{
			_log.DebugFormat(message, args);
		}

		public void Info(string message, params object[] args)
		{
			_log.InfoFormat(message, args);
		}

		public void Warn(string message, params object[] args)
		{
			_log.WarnFormat(message, args);
		}

		public void Error(string message, params object[] args)
		{
			_log.ErrorFormat(message, args);
		}

		public void Fatal(string message, params object[] args)
		{
			_log.FatalFormat(message, args);
		}

        public void Trace(string messageTemplate, params object[] args)
        {
           _log.DebugFormat(messageTemplate, args); 
        }

        public void Trace(Exception exception)
        {
            _log.Debug("Exception", exception);
        }

        public void Trace(Func<string> messageGenerator)
        {
            if(_log.IsDebugEnabled) _log.Debug(messageGenerator.Invoke());
        }

        public void Debug(Exception exception)
        {
            _log.Debug("Exception", exception);
        }

        public void Debug(Func<string> messageGenerator)
        {
            if(_log.IsDebugEnabled) _log.Debug(messageGenerator.Invoke());
        }

        public void Info(Exception exception)
        {
            _log.Info("Exception", exception);
        }

        public void Info(Func<string> messageGenerator)
        {
            if (_log.IsInfoEnabled)
            {
                _log.Info(messageGenerator.Invoke());
            }
        }

        public void Warn(Exception exception)
        {
            _log.Warn("Exception", exception);
        }

        public void Warn(Func<string> messageGenerator)
        {
            if (_log.IsWarnEnabled)
            {
                _log.Warn(messageGenerator.Invoke());
            }
        }

        public void Error(Exception exception)
        {
            _log.Error("Exception", exception);
        }

        public void Error(Func<string> messageGenerator)
        {
            if (_log.IsErrorEnabled)
            {
                _log.Error(messageGenerator.Invoke());
            }
        }

        public void Fatal(Exception exception)
        {
            _log.Fatal("Exception", exception);
        }

        public void Fatal(Func<string> messageGenerator)
        {
            if (_log.IsFatalEnabled)
            {
                _log.Fatal(messageGenerator.Invoke());
            }
        }
    }
}
