using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace LiveDomain.Core.Proxy
{
	[Serializable]
    public class ProxyCommand<M> : CommandWithResult<M, object> where M : Model
	{
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }

		public ProxyCommand(string methodName, object[] inArgs)
		{
			MethodName = methodName;
			Arguments = inArgs;
		}

		#region Overrides of CommandWithResult<T,object>

		protected internal override object Execute(M model)
		{
		    var proxyMethod = ProxyMethodMap.GetProxyMethodMap<M>().GetProxyMethodInfo(MethodName);
		    MethodInfo methodInfo = proxyMethod.MethodInfo;
			return   methodInfo.Invoke(model, Arguments);
		}

		#endregion
	}
}
