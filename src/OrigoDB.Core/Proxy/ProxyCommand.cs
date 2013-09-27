using System;
using System.Reflection;

namespace OrigoDB.Core.Proxy
{
	[Serializable]
    public class ProxyCommand<TModel> : CommandWithResult<TModel, object> where TModel : Model
	{
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }

		public ProxyCommand(string methodName, object[] inArgs)
		{
			MethodName = methodName;
			Arguments = inArgs;
		}

		protected internal override object Execute(TModel model)
		{
		    var proxyMethod = ProxyMethodMap.MapFor<TModel>().GetProxyMethodInfo(MethodName);
		    MethodInfo methodInfo = proxyMethod.MethodInfo;
			return methodInfo.Invoke(model, Arguments);
		}
	}
}
