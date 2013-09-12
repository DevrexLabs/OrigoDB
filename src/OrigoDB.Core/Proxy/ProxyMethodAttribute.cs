using System;

namespace OrigoDB.Core.Proxy
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class ProxyMethodAttribute : Attribute
	{
		public OperationType OperationType { get; set; }
	    
        /// <summary>
	    /// When set to true, tells the engine there is no way to modify the model through references
	    /// contained in the return value nor can the results be modified by a subsequent command.
	    /// <remarks>This is achieved by cloning, returning immutable objects or both</remarks>
	    /// </summary>
        public bool ResultIsSafe { get; set; }

	}
}