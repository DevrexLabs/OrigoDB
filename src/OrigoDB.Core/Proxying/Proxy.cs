using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using OrigoDB.Core;

namespace Proxying
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
            if (!operationInfo.IsAllowed) throw new Exception("Method not allowed");
            var result = operationInfo.Execute(_handler, methodCall, signature);
            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
        }

        internal static string GetSignature(IMethodCallMessage callMessage)
        {
            var argTypes = callMessage.InArgs.Select(a => a.GetType()).ToArray();
            // ReSharper disable once PossibleNullReferenceException
            return Type.GetType(callMessage.TypeName)
                .GetMethod(callMessage.MethodName, argTypes).ToString();
        }
    }
}
