using System;

namespace OrigoDB.Core.Proxy
{
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property)]
    public class ProxyAttribute : Attribute
    {
        internal OperationType Operation { get; set; }


        public ProxyAttribute()
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