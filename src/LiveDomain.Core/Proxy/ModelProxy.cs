using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Collections.Generic;

namespace LiveDomain.Core.Proxy
{
    public class ModelProxy<T> : RealProxy where T : Model
    {
        readonly IEngine<T> _handler;
        private readonly ProxyMethodMap<T> _proxyMethods;

        public ModelProxy(IEngine<T> handler)
            : base(typeof(T))
        {
            _handler = handler;
            _proxyMethods = ProxyMethodMap.GetProxyMethodMap<T>(); 
        }

        public override IMessage Invoke(IMessage myIMessage)
        {
            var methodCall = myIMessage as IMethodCallMessage;

            if (methodCall == null)
            {
                throw new NotSupportedException("Only methodcalls supported");
            }
            
            var proxyInfo = _proxyMethods.GetProxyMethodInfo(methodCall.MethodName);

            object result;
            if (proxyInfo.IsCommand)
            {
                var command = new ProxyCommand<T>(methodCall.MethodName, methodCall.InArgs);
                command.EnsuresResultIsDisconnected = proxyInfo.ProxyMethodAttribute.EnsuresResultIsDisconnected;
                result = _handler.Execute(command);
            }
            else if (proxyInfo.IsQuery)
            {
                var query = new ProxyQuery<T>(methodCall.MethodName, methodCall.InArgs);
                query.EnsuresResultIsDisconnected = proxyInfo.ProxyMethodAttribute.EnsuresResultIsDisconnected;
                result = _handler.Execute(query);
            }
            else throw new InvalidEnumArgumentException("TransactionType not initialized");

            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
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
