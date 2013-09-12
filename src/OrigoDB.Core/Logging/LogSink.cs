using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core.Logging
{
    public abstract class LogSink
    {
        readonly LogFormatter _formatter;

        public LogSink(LogFormatter formatter = null)
        {
            _formatter = formatter ?? new LogFormatter();
        }

        public virtual void Write(string logger, LogLevel level, string message)
        {
            string formattedMessage = _formatter.Format(logger, level, message);
            WriteMessage(formattedMessage);
        }

        public abstract void WriteMessage(string formattedMessage);
    }
}
