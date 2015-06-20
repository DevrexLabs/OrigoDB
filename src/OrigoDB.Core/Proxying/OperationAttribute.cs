using System;

namespace Proxying
{
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property)]
    public class OperationAttribute : Attribute
    {
        internal OperationType Type { get; set; }


        public OperationAttribute()
        {
            CloneResult = true;
        }

        /// <summary>
        /// Result of this method call will be cloned unless immutable.
        /// </summary>
        public bool CloneResult { get; set; }

        /// <summary>
        /// Map to an explict Command or Query type or the generic proxy types if null
        /// </summary>
        public Type MapTo { get; set; }
    }
}