using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Modules.Log4Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modules.Log4Net.Test
{
	[TestClass]
	public class Log4NetFactoryTest
	{
		[TestMethod()]
		public void Can_create_log4netlogfactory()
		{
			new Log4NetLogFactory();
		}

		[TestMethod()]
		public void Can_create_log4netlogger()
		{
			new Log4NetLogFactory().GetLoggerForCallingType();
		}

		[TestMethod()]
		public void Can_log_message()
		{
			new Log4NetLogFactory().GetLoggerForCallingType().Info("Hello, world!");
		}
	}
}
