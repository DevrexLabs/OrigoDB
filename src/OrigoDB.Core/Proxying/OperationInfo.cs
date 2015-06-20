using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using OrigoDB.Core;

namespace Proxying
{
    internal abstract class OperationInfo<T> where T : Model
    {

        public readonly MethodInfo MethodInfo;
        public readonly OperationAttribute OperationAttribute;

        protected OperationInfo(MethodInfo methodInfo, OperationAttribute operationAttribute)
        {
            MethodInfo = methodInfo;
            OperationAttribute = operationAttribute;
        }

        public bool IsAllowed
        {
            get
            {
                return OperationAttribute.Type != OperationType.Disallowed;
            }
        }

        protected bool IsMapped
        {
            get { return OperationAttribute.MapTo != null; }
        }

        private object GetMappedOperation(IMethodCallMessage callMessage)
        {
            var mapTo = OperationAttribute.MapTo;
            var constructor = mapTo.GetConstructor(callMessage.InArgs.Select(args => args.GetType()).ToArray());
            if (constructor == null) return null;
            return constructor.Invoke(callMessage.InArgs);
        }

        public object Execute(IEngine<T> engine, IMethodCallMessage callMessage, string signature)
        {
            var operation = IsMapped ? GetMappedOperation(callMessage) : null;
            return Execute(engine, signature, operation, callMessage.InArgs);
        }

        protected abstract object Execute(IEngine<T> engine, string signature, object operation, object[] args);

        public static OperationInfo<T> Create(MethodInfo methodInfo, OperationAttribute proxyAttribute)
        {
            if (proxyAttribute.Type == OperationType.Command) return new CommandInfo<T>(methodInfo, proxyAttribute);
            if (proxyAttribute.Type == OperationType.Query) return new QueryInfo<T>(methodInfo,proxyAttribute);
            throw new ArgumentException("Expected operation Command or Query", "proxyAttribute");
        }
    }
}