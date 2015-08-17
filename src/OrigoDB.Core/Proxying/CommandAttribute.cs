using System;
using System.Reflection;
using OrigoDB.Core.Proxying;

namespace OrigoDB.Core
{
    /// <summary>
    /// Used to mark non-void methods as commands so they won't be interpreted as queries.
    /// Can also be used to map methods to a domain specific command type
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandAttribute : OperationAttribute
    {
        public readonly static OperationAttribute Default = new CommandAttribute();

        public CommandAttribute()
        {
            Type = OperationType.Command;
        }

        public CommandAttribute(IsolationLevel isolation) : this()
        {
            Isolation = isolation;
        }

        /// <summary>
        /// Before overloads were introduced, methods were identified by name only.
        /// If you have any of these in your journal set this to true for the
        /// overload you want to map them to. There can be only one.
        /// </summary>
        public bool IsDefault{ get; set; }

        internal override OperationInfo<T> ToOperationInfo<T>(MethodInfo methodInfo)
        {
            return new CommandInfo<T>(methodInfo,this);
        }
    }
}