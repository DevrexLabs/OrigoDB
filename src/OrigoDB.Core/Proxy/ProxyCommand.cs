using System;
using System.Reflection;

namespace OrigoDB.Core.Proxy
{
	[Serializable]
    public class ProxyCommand<TModel> : Command<TModel, object> where TModel : Model
	{
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }

		public ProxyCommand(string methodName, object[] inArgs)
		{
			MethodName = methodName;
			Arguments = inArgs;
		}

		public override object Execute(TModel model)
		{
		    var proxyMethod = ProxyMethodMap.MapFor<TModel>().GetProxyMethodInfo(MethodName);
		    MethodInfo methodInfo = proxyMethod.MethodInfo;
			return methodInfo.Invoke(model, Arguments);
		}
	}
}
