using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace OrigoDB.Core.Proxy
{
    public class ModelProxy<TModel> : RealProxy where TModel : Model
    {
        readonly IEngine<TModel> _handler;
        private readonly MethodMap _methods;

        public ModelProxy(IEngine<TModel> handler)
            : base(typeof(TModel))
        {
            _handler = handler;
            _methods = MethodMap.MapFor<TModel>();
        }

        public override IMessage Invoke(IMessage myIMessage)
        {
            var methodCall = myIMessage as IMethodCallMessage;
            if (methodCall == null)
            {
                throw new NotSupportedException("Only methodcalls supported");
            }

            var signature = GetSignature(methodCall);
            var proxyInfo = _methods.GetProxyMethodInfo(signature);

            object result;
            if (proxyInfo.IsCommand)
            {
                var command = new ProxyCommand<TModel>(signature, methodCall.InArgs);
                command.ResultIsSafe = !proxyInfo.ProxyAttribute.CloneResult;
                result = _handler.Execute(command);
            }
            else if (proxyInfo.IsQuery)
            {
                var query = new ProxyQuery<TModel>(signature, methodCall.InArgs);
                query.ResultIsSafe = !proxyInfo.ProxyAttribute.CloneResult;
                result = _handler.Execute(query);
            }
            else throw new InvalidEnumArgumentException("OperationType not initialized");

            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);

        }

        private static string GetSignature(IMethodCallMessage callMessage)
        {
            var argTypes = callMessage.InArgs.Select(a => a.GetType()).ToArray();
            // ReSharper disable once PossibleNullReferenceException
            return Type.GetType(callMessage.TypeName)
                .GetMethod(callMessage.MethodName, argTypes).ToString();

        }
    }
}
