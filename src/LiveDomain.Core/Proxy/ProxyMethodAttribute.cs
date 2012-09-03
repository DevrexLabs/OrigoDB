using System;

namespace LiveDomain.Core.Proxy
{
	public enum ProxyMethodType
	{
		Command,
		Query
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class ProxyMethodAttribute : Attribute
	{
	
		public ProxyMethodType MethodType { get; private set; }
		public ProxyMethodAttribute(ProxyMethodType proxyMethodType)
		{
			MethodType = proxyMethodType;
		}
	}
}