using System;

namespace LiveDomain.Core.Proxy
{
	public enum TransactionType
	{
        Unspecified,
		Command,
		Query
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class ProxyMethodAttribute : Attribute
	{
		public TransactionType TransactionType { get; set; }
	    public bool EnsuresResultIsDisconnected { get; set; }

	}
}