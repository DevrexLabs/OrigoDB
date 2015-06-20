using System;

namespace Proxying
{
    /// <summary>
    /// Used to mark non-void methods as commands so they won't be interpreted as queries.
    /// Can also be used to map methods to a domain specific command type
    /// </summary>
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class CommandAttribute : OperationAttribute
    {
        public readonly static OperationAttribute Default = new CommandAttribute();

        public CommandAttribute()
        {
            Type = OperationType.Command;
        }

    }
}