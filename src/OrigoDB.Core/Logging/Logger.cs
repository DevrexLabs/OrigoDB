using System;

namespace OrigoDB.Core.Logging
{
    public abstract class Logger : ILogger
    {

        public virtual void Trace(string message, params object[] args)
        {
            Write(LogLevel.Trace, message, args);
        }

        public virtual void Trace(Exception exception)
        {
            Write(LogLevel.Trace, exception);
        }

        public virtual void Trace(Func<string> messageGenerator)
        {
            Write(LogLevel.Trace, messageGenerator);
        }

        public virtual void Debug(string message, params object[] args)
        {
            Write(LogLevel.Debug, message, args);
        }

        public virtual void Debug(Exception exception)
        {
            Write(LogLevel.Debug, exception);
        }

        public virtual void Debug(Func<string> messageGenerator)
        {
            Write(LogLevel.Debug, messageGenerator);
        }

        public virtual void Info(string message, params object[] args)
        {
            Write(LogLevel.Info, message, args);
        }

        public virtual void Info(Exception exception)
        {
            Write(LogLevel.Info, exception); 
        }

        public virtual void Info(Func<string> messageGenerator)
        {
            Write(LogLevel.Info, messageGenerator);
        }

        public virtual void Warn(string message, params object[] args)
        {
            Write(LogLevel.Warn, message,args);
        }

        public virtual void Warn(Exception exception)
        {
            Write(LogLevel.Warn, exception);
        }

        public virtual void Warn(Func<string> messageGenerator)
        {
            Write(LogLevel.Warn, messageGenerator);
        }

        public virtual void Error(string message, params object[] args)
        {
            Write(LogLevel.Error, message, args);
        }

        public virtual void Error(Exception exception)
        {
            Write(LogLevel.Error, exception); 
        }

        public virtual void Error(Func<string> messageGenerator)
        {
            Write(LogLevel.Error, messageGenerator);
        }

        public virtual void Fatal(string message, params object[] args)
        {
            Write(LogLevel.Fatal, message,args);
        }

        public virtual void Fatal(Exception exception)
        {
            Write(LogLevel.Fatal, exception);
        }

        public virtual void Fatal(Func<string> messageGenerator)
        {
            Write(LogLevel.Fatal, messageGenerator);
        }


        protected virtual void Write(LogLevel level, string message, params object[] args)
        {
            Write(level, () => string.Format(message, args));
        }

        protected virtual void Write(LogLevel level, Exception exception)
        {
            Write(level, () => string.Format("[{0}] message=[{1}]", exception.GetType().Name, exception.Message));
        }

        protected abstract void Write(LogLevel level, Func<string> messageGenerator);

    }
}