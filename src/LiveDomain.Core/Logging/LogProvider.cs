using System;
using System.IO;
using LiveDomain.Core.Utilities;

namespace LiveDomain.Core.Logging
{
    public static class LogProvider
    {
        private static ILogFactory _logFactory;
        private static readonly object locker = new object();

        /// <summary>
        /// Can only be set once and must be called before the first invocation of LogProvider.Factory
        /// </summary>
        /// <param name="logFactory"></param>
        public static void SetLogFactory(ILogFactory logFactory)
        {
            Ensure.NotNull(logFactory, "logFactory");
            if(_logFactory != null) 
                throw new InvalidOperationException("Log factory can only be set once and must happen before the first call to LogProvider.Factory");
            _logFactory = logFactory;
        }

        public static ILogFactory Factory
        {
            get
            {
                lock (locker)
                {
                    return _logFactory =_logFactory ?? new LogFactory();
                }
            }
        }

        public static string GetDefaultLogFile()
        {
            var directory = StorageLocation.GetDefaultDirectory();
            return Path.Combine(directory, "log.txt");
        }
    }
}