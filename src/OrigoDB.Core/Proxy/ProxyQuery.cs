using System;

namespace OrigoDB.Core.Proxy
{
	[Serializable]
	public class ProxyQuery<TModel> : Query<TModel,object> where TModel : Model
	{
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }

		public ProxyQuery(string methodName, object[] inArgs)
		{
			MethodName = methodName;
			Arguments = inArgs;
		}

		public override object Execute(TModel model)
		{
            var proxyMethod = ProxyMethodMap.MapFor<TModel>().GetProxyMethodInfo(MethodName);
		    var method = proxyMethod.MethodInfo;
			return method.Invoke(model, Arguments);
		}
	}
}