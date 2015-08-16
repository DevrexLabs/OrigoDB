using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace OrigoDB.Core.Proxying
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

        public bool? InputIsIsolated
        {
            get
            {
                if (OperationAttribute.Isolation.HasFlag(Isolation.Input)) return true;
                return null;
            }
        }

        public bool? ResultIsIsolated
        {
            get
            {
                if (OperationAttribute.Isolation.HasFlag(Isolation.Output)) return true;
                return null;
            }
        }

        protected bool IsMapped
        {
            get { return OperationAttribute.MapTo != null; }
        }

        /// <summary>
        /// If operation attribute had a MapTo property selecting a Command or
        /// Query type to map to, return an instance of that type, otherwise null
        /// </summary>
        /// <returns></returns>
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
            return Execute(engine, signature, operation, callMessage);
        }

        protected abstract object Execute(IEngine<T> engine, string signature, object operation, IMethodCallMessage methodCall);
    }
}