using System;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Logging
{
    public static class LogProvider
    {
        private static ILoggerFactory _logFactory;
        private static readonly object locker = new object();

        /// <summary>
        /// Can only be set once and must be called before the first invocation of LogProvider.Factory
        /// </summary>
        /// <param name="logFactory"></param>
        public static void SetLogFactory(ILoggerFactory logFactory)
        {
            Ensure.NotNull(logFactory, "logFactory");
            if(_logFactory != null) 
                throw new InvalidOperationException("Log factory can only be set once and must happen before the first call to LogProvider.Factory");
            _logFactory = logFactory;
        }

        public static ILoggerFactory Factory
        {
            get
            {
                lock (locker)
                {
                    return _logFactory =_logFactory ?? new NullLoggerFactory();
                }
            }
        }
    }
}