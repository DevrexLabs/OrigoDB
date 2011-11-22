using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    public abstract class Logger : ILog
    {
        public void Debug(string message)
        {
            Write(LogMessageType.Debug, message);
        }

        public void Write(string message)
        {
            Write(LogMessageType.Info, message);
        }

        public void Warn(string message)
        {
            Write(LogMessageType.Warning, message);
        }

        public void Write(Exception exception)
        {
            string message = BuildMessageFromException(exception);
            Write(LogMessageType.Exception, message);
        }

        public virtual void Dispose()
        {
        }

        private string BuildMessageFromException(Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            while (exception != null)
            {
                builder.AppendLine(exception.Message);
                builder.AppendLine(exception.StackTrace);
                exception = exception.InnerException;
            }
            return builder.ToString();

        }

        protected virtual string FormatMessage(LogMessageType messageType, string message)
        {
            return DateTime.Now.ToString() + " : " + messageType.ToString().ToUpper() + " : " + message;
        }

        protected virtual void Write(LogMessageType messageType, string message)
        {
            string formattedMessage = FormatMessage(messageType, message);
            WriteToLog(message);
        }


        protected abstract void WriteToLog(string message);
    }
}
