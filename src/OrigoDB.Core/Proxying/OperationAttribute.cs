using System;
using System.Reflection;
using OrigoDB.Core.Proxying;


namespace OrigoDB.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class OperationAttribute : Attribute
    {
        internal OperationType Type { get; set; }


        protected OperationAttribute()
        {
            ResultIsIsolated = false;
        }

        /// <summary>
        /// Result of this method call will be cloned unless marked isolated.
        /// </summary>
        public bool ResultIsIsolated { get; set; }

        /// <summary>
        /// Map to an explict Command or Query type or the generic proxy types if null
        /// </summary>
        public Type MapTo { get; set; }

        internal abstract OperationInfo<T> ToOperationInfo<T>(MethodInfo methodInfo) where T : Model;
    }
}