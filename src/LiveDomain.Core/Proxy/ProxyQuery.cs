using System;

namespace LiveDomain.Core.Proxy
{
	[Serializable]
	public class ProxyQuery<M> : Query<M,object> where M : Model
	{
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }

		public ProxyQuery(string methodName, object[] inArgs)
		{
			MethodName = methodName;
			Arguments = inArgs;
		}

		protected override object Execute(M m)
		{
            var proxyMethod = ProxyMethodMap.GetProxyMethodMap<M>().GetProxyMethodInfo(MethodName);
		    var method = proxyMethod.MethodInfo;
			return method.Invoke(m, Arguments);
		}
	}
}