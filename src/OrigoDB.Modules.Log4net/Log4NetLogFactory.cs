using System;
using System.Diagnostics;
using System.IO;
using OrigoDB.Core.Logging;
using log4net;
using log4net.Config;
using ILog = OrigoDB.Core.Logging.ILogger;

namespace OrigoDB.Modules.Log4Net
{
	public class Log4NetLogFactory : ILoggerFactory
	{
		public Log4NetLogFactory(string configurationPath = null)
		{
			var configFile = new FileInfo(configurationPath ?? AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
			XmlConfigurator.ConfigureAndWatch(configFile);
		}

		public ILog GetLogger(Type type)
		{
			var log4NetLog = LogManager.GetLogger(type);
			var log = new Log4NetLogAdapter(log4NetLog);
			return log;
		}

		public ILog GetLoggerForCallingType()
		{
			return GetLogger(new StackFrame(1,false).GetMethod().DeclaringType);
		}
	}
}