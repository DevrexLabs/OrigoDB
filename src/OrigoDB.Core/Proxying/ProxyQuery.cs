using System;
using OrigoDB.Core;

namespace Proxying
{
	[Serializable]
	public class ProxyQuery<T> : Query<T,object> where T : Model
	{
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }

		public ProxyQuery(string methodName, object[] inArgs)
		{
			MethodName = methodName;
			Arguments = inArgs;
		}

		public override object Execute(T model)
		{
            var proxyMethod = MethodMap.MapFor<T>().GetOperationInfo(MethodName);
		    var method = proxyMethod.MethodInfo;
			return method.Invoke(model, Arguments);
		}
	}
}