using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    public enum LogMessageType
    {
        Debug,
        Info,
        Warning,
        Exception
    }

    public interface ILog
    {
        void Debug(string message);
        void Write(string message);
        void Warn(string message);
        void Write(Exception exception);
    }

    public class NullLogger : Logger
    {
        protected override void Write(LogMessageType type, string message)
        {
        }
    }

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

        protected abstract void Write(LogMessageType messageType, string message);
    }

    public class InMemoryLogger : Logger
    {
        List<string> _messages = new List<string>();
        public IEnumerable<string> Messages { get { return _messages; } }


        protected override void Write(LogMessageType messageType, string message)
        {
            _messages.Add(DateTime.Now.ToString() + " : " + messageType.ToString().ToUpper() + " : " + message);
        }
    }
}
