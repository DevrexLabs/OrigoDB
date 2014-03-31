using System;

namespace OrigoDB.Core.Proxy
{
    [Obsolete("Use CommandAttribute, QueryAttribute or ProxyAttribute instead")]
    public class ProxyMethodAttribute : Attribute
    {
        public OperationType OperationType { get; set; }

        /// <summary>
        /// When set to true, tells the engine there is no way to modify the model through references
        /// contained in the return value nor can the results be modified by a subsequent command.
        /// <remarks>This is achieved by cloning, returning immutable objects or both</remarks>
        /// </summary>
        public bool ResultIsSafe { get; set; }
    }


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


    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class QueryAttribute : ProxyAttribute
    {
        public QueryAttribute()
        {
            Operation = OperationType.Query;
        }
    }


    
}