using System;

namespace OrigoDB.Core.Logging
{
    public class ConsoleLogger : Logger
    {
        public static LogLevel MinimumLevel = LogLevel.Info;

        public readonly string Name;

        public ConsoleLogger(string name)
        {
            Name = name;
        }

        protected override void Write(LogLevel level, Func<string> messageGenerator)
        {
            if (level >= MinimumLevel)
            {
                const string logLineTemplate = "{0} - {1} - {2} - {3}";
                Console.WriteLine(logLineTemplate, DateTime.Now, level.ToString().ToUpper(), Name, messageGenerator.Invoke());
            }
        }
    }
}