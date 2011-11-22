using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    /// <summary>
    /// Logging facade
    /// </summary>
    public static class Log
    {

        static ILog _logger = new NullLogger();

        public static void SetLogger(ILog logger)
        {
            _logger = logger;
        }

        public static void Write(string message)
        {
            _logger.Write(message);
        }

        public static void Warn(string message)
        {
            _logger.Warn(message);
        }

        public static void Debug(string message)
        {
            _logger.Debug(message);
        }

        public static void Write(Exception exception)
        {
            _logger.Write(exception);
        }
    }
}
