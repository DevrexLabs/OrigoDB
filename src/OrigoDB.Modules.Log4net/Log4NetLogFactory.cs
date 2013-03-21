using System;
using System.Diagnostics;
using System.IO;
using OrigoDB.Core.Logging;
using log4net;
using log4net.Config;
using ILog = OrigoDB.Core.Logging.ILog;

namespace OrigoDB.Modules.Log4Net
{
	public class Log4NetLogFactory : ILogFactory
	{
		public Log4NetLogFactory(string configurationPath = null)
		{
			var configFile = new FileInfo(configurationPath ?? AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
			XmlConfigurator.ConfigureAndWatch(configFile);
		}

		public ILog GetLog(Type type)
		{
			var log4NetLog = LogManager.GetLogger(type);
			var log = new Log4NetLogAdapter(log4NetLog);
			return log;
		}

		public ILog GetLogForCallingType()
		{
			return GetLog(new StackFrame(1,false).GetMethod().DeclaringType);
		}
	}
}