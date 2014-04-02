using System;

namespace OrigoDB.Core.Proxy
{
    /// <summary>
    /// Used to mark non-void methods as commands so they won't be interpreted as queries.
    /// Can also be used to map methods to a domain specific command type
    /// </summary>
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class CommandAttribute : ProxyAttribute
    {
        public CommandAttribute()
        {
            Operation = OperationType.Command;
        }

    }
}