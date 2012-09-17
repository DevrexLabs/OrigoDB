using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace LiveDomain.Core.Proxy
{
	public class ModelProxy<T> : RealProxy where T : Model
	{
		IEngine<T> _handler;  

        public ModelProxy(IEngine<T> handler) : base(typeof(T))
        {
        	_handler = handler;
        }
		
        public override IMessage Invoke(IMessage myIMessage)
        {
            var methodCall = myIMessage as IMethodCallMessage;

			if (methodCall == null) // Todo : filter events etc.
				throw new NotSupportedException("Only methodcalls supported");

        	var methodInfo = ReflectionHelper.ResolveMethod(typeof(T), methodCall.MethodName);

        	object result;
			if (GetProxyType(methodInfo) == ProxyMethodType.Command)
			{
				var command = new ReflectionCommand<T>(methodCall.MethodName, methodCall.InArgs);
				result = _handler.Execute(command);
			}
			else
			{
				var query = new ReflectionQuery<T>(methodCall.MethodName, methodCall.InArgs);
				result = _handler.Execute(query);
			}
			
			return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);            	
        }

		ProxyMethodType GetProxyType(MethodInfo methodInfo)
		{
			var attributes = methodInfo.GetCustomAttributes(typeof(ProxyMethodAttribute), false);
			if(attributes.Length == 1) return ((ProxyMethodAttribute) attributes[0]).MethodType;
			return methodInfo.ReturnType == typeof(void) ? ProxyMethodType.Command : ProxyMethodType.Query;
		}
	}

	public static class ModelExtensions
	{
		 public static T GetProxy<T>(this IEngine<T> instance) where T : Model
		 {
		 	return (T)new ModelProxy<T>(instance).GetTransparentProxy();
		 }
	}
}
