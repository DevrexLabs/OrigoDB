using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrigoDB.Core.Logging;

namespace OrigoDB.Modules.Log4Net
{
	public class Log4NetLogAdapter : ILog
	{
		readonly log4net.ILog _log;

		public Log4NetLogAdapter(log4net.ILog log)
		{
			_log = log;
		}

		public void Debug(string message, params object[] args)
		{
			_log.Debug(string.Format(message,args));
		}

		public void Info(string message, params object[] args)
		{
			_log.Info(string.Format(message, args));
		}

		public void Warn(string message, params object[] args)
		{
			_log.Warn(string.Format(message, args));
		}

		public void Error(string message, params object[] args)
		{
			_log.Error(string.Format(message, args));
		}

		public void Exception(Exception exception)
		{
			_log.Error("Exception",exception);
		}

		public void Fatal(string message, params object[] args)
		{
			_log.Fatal(string.Format(message, args));
		}
	}
}
