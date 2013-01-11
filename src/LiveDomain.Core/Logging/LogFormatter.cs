using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using LiveDomain.Core.Utilities;

namespace LiveDomain.Core.Logging
{
    public class LogFormatter
    {
        public const string DefaultTemplate = "{date} - {level} - {logger} - {message}";
        public readonly string FormattingTemplate;
        private static readonly int levelPadding;

        static LogFormatter()
        {
            levelPadding = Enum.GetNames(typeof (LogLevel)).Max(name => name.Length);
        }

        public LogFormatter(string template = DefaultTemplate)
        {
            Ensure.NotNullOrEmpty(template, "template");
            FormattingTemplate = template;

        }

        public virtual string Format(string logger, LogLevel logLevel, String message)
        {
            Func<Match, String> matchEvaluator = (m) =>
                {
                    string key = m.Groups["key"].Value.ToLower();
                    switch (key)
                    {
                        case "message":
                            return message;
                        case "thread":
                            return GetThreadName();
                        case "level":
                            return logLevel.ToString().ToUpper().PadRight(levelPadding);
                        case "date":
                            return DateTime.Now.ToString();
                        case "logger":
                            return logger;
                        case "tab":
                            return "\t";
                        case "nl":
                            return Environment.NewLine;
                        default:
                            return "{" + key + "}";
                    }
                };

            return Regex.Replace(FormattingTemplate + "{nl}", @"{(?<key>[a-z]+)}", matchEvaluator.Invoke);
        }

        internal string GetThreadName()
        {
            string threadName = Thread.CurrentThread.Name;
            if (String.IsNullOrWhiteSpace(threadName))
            {
                threadName = Thread.CurrentThread.ManagedThreadId.ToString();
            }
            return threadName;
        }
    }
}
