using System;
using System.Linq;
using System.Text;
using OrigoDB.Core.Logging;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Logging
{
    public sealed class Log : ILog
    {
        public readonly String Name;
        private readonly LogKernel _kernel;

        internal Log(LogKernel kernel, string name)
        {
            Ensure.NotNullOrEmpty(name, "name");
            Ensure.NotNull(kernel, "kernel");
            Name = name;
            _kernel = kernel;
        }


        public void Debug(string message, params object[] args)
        {
            Dispatch(LogLevel.Debug, message, args);
        }

        public void Info(string message, params object[] args)
        {
            Dispatch(LogLevel.Info, message, args);
        }

        public void Warn(string message, params object[] args)
        {
            Dispatch(LogLevel.Warn, message, args);
        }

        public void Error(string message, params object[] args)
        {
            Dispatch(LogLevel.Error, message, args);
        }

        public void Exception(Exception exception)
        {
            string message = BuildMessageFromException(exception);
            Dispatch(LogLevel.Error, message);
        }

        public void Fatal(string message, params object[] args)
        {
            Dispatch(LogLevel.Fatal, message, args);
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

        private void Dispatch(LogLevel logLevel, string messageTemplate, params object[] args)
        {
            _kernel.Dispatch(Name, logLevel, messageTemplate, args);
        }
    }
}
