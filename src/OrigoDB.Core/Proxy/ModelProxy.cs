using System;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace OrigoDB.Core.Proxy
{

    public class ModelProxy<TModel> : RealProxy where TModel : Model
    {
        readonly IEngine<TModel> _handler;
        private readonly ProxyMethodMap _proxyMethods;

        public ModelProxy(IEngine<TModel> handler)
            : base(typeof(TModel))
        {
            _handler = handler;
            _proxyMethods = ProxyMethodMap.MapFor<TModel>();
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
                var command = new ProxyCommand<TModel>(methodCall.MethodName, methodCall.InArgs);
                command.ResultIsSafe = !proxyInfo.ProxyAttribute.CloneResult;
                result = _handler.Execute(command);
            }
            else if (proxyInfo.IsQuery)
            {
                var query = new ProxyQuery<TModel>(methodCall.MethodName, methodCall.InArgs);
                query.ResultIsSafe = !proxyInfo.ProxyAttribute.CloneResult;
                result = _handler.Execute(query);
            }
            else throw new InvalidEnumArgumentException("OperationType not initialized");

            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);

        }
    }
}
