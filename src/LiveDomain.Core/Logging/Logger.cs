using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core.Logging;

namespace LiveDomain.Core
{
    public abstract class Logger : ILog
    {

        public void Trace(string message, params object[] args)
        {
            Write(LogMessageType.Trace, message, args);
        }

        public void Debug(string message, params object[] args)
        {
            Write(LogMessageType.Debug, message, args);
        }

        public void Info(string message, params object[] args)
        {
            Write(LogMessageType.Info, message, args);
        }

        public void Warn(string message, params object[] args)
        {
            Write(LogMessageType.Warning, message, args);
        }

        public void Error(string message, params object[] args)
        {
            Write(LogMessageType.Error, message, args);
        }

        public void Exception(Exception exception)
        {
            string message = BuildMessageFromException(exception);
            Write(LogMessageType.Error, message);
        }

        public void Fatal(string message, params object[] args)
        {
            Write(LogMessageType.Fatal, message, args);
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

        protected virtual void Write(LogMessageType messageType, string message, params object[] args)
        {
            message = String.Format(message, args);
            string formattedMessage = FormatMessage(messageType, message);
            lock(this) WriteToLog(message);
        }


        protected abstract void WriteToLog(string message);
    }
}
