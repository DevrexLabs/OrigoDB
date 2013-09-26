namespace OrigoDB.Core.Logging
{
    public static class LogProvider
    {
        private static ILoggerFactory _factory = new ConsoleLoggerFactory();

        public static ILoggerFactory Factory
        {
            get
            {
               return _factory;
            }
        }

        public static void SetFactory(ILoggerFactory factory)
        {
            _factory = factory;
        }
    }
}