using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Collections.Generic;

namespace LiveDomain.Core.Proxy
{

    internal class ModelProxy<T> : ModelProxy<T, T> where T : Model
    {
        public ModelProxy(IEngine<T> handler)
            : base(handler)
        {

        }
    }

    public class ModelProxy<P, C> : RealProxy
        where P : Model
        where C : Model
    {
        readonly IEngine<P> _handler;
        private readonly ProxyMethodMap _proxyMethods;

        public ModelProxy(IEngine<P> handler)
            : base(typeof(C))
        {
            _handler = handler;
            _proxyMethods = ProxyMethodMap.MapFor<C>();
        }

        public override IMessage Invoke(IMessage myIMessage)
        {
            var methodCall = myIMessage as IMethodCallMessage;

            if (methodCall == null)
            {
                throw new NotSupportedException("Only methodcalls supported");
            }

            if (methodCall.MethodName == "ChildFor")
            {
                return ProxyForChild(methodCall);
            }

            var proxyInfo = _proxyMethods.GetProxyMethodInfo(methodCall.MethodName);

            object result;
            if (proxyInfo.IsCommand)
            {
                var command = new ProxyCommand<C>(methodCall.MethodName, methodCall.InArgs);
                command.ResultIsSafe = proxyInfo.ProxyMethodAttribute.ResultIsSafe;
                result = _handler.Execute(command);
            }
            else if (proxyInfo.IsQuery)
            {
                var query = new ProxyQuery<C>(methodCall.MethodName, methodCall.InArgs);
                query.ResultIsSafe = proxyInfo.ProxyMethodAttribute.ResultIsSafe;
                result = _handler.Execute<C,object>(query);
            }
            else throw new InvalidEnumArgumentException("OperationType not initialized");

            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
        }

        private IMessage ProxyForChild(IMethodCallMessage methodCall)
        {
            Type proxyType = typeof (ModelProxy<,>);
            Type childModelType = methodCall.MethodBase.GetGenericArguments().First();
            Type[] genericTypeArgs = new Type[] {typeof (P), childModelType};
            Type genericProxyType = proxyType.MakeGenericType(genericTypeArgs);
            object[] constructorArgs = new object[] {_handler};
            RealProxy childProxy = (RealProxy)Activator.CreateInstance(genericProxyType, constructorArgs);
            return new ReturnMessage(childProxy.GetTransparentProxy(), null, 0, methodCall.LogicalCallContext, methodCall);
        }
    }

    public static class ModelProxyExtensions
    {
        public static M GetProxy<M>(this IEngine<M> engine) where M : Model
        {
            return (M)new ModelProxy<M>(engine).GetTransparentProxy();
        }
    }

}
