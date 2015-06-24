using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace OrigoDB.Core.Proxying
{
    public class Proxy<T> : RealProxy where T : Model
    {
        readonly IEngine<T> _handler;
        private readonly MethodMap<T> _methods;

        public Proxy(IEngine<T> handler)
            : base(typeof(T))
        {
            _handler = handler;
            _methods = MethodMap.MapFor<T>();
        }

        public override IMessage Invoke(IMessage myIMessage)
        {
            var methodCall = myIMessage as IMethodCallMessage;
            if (methodCall == null)
            {
                throw new NotSupportedException("Only methodcalls supported");
            }

            var signature = GetSignature(methodCall);
            var operationInfo = _methods.GetOperationInfo(signature);
            var result = operationInfo.Execute(_handler, methodCall, signature);
            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
        }

        private static string GetSignature(IMethodMessage callMessage)
        {
            var argTypes = callMessage.MethodBase
                .GetParameters()
                .Select(pi => pi.ParameterType).ToArray();
            return typeof(T).GetMethod(callMessage.MethodName, argTypes).ToString();
        }
    }
}
