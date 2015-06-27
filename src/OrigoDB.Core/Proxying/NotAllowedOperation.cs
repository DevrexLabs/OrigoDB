using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;


namespace OrigoDB.Core.Proxying
{
    internal class NotAllowedOperation<T> : OperationInfo<T> where T : Model
    {
        public NotAllowedOperation(MethodInfo methodInfo, OperationAttribute operationAttribute) 
            : base(methodInfo, operationAttribute)
        {
        }


        protected override object Execute(IEngine<T> engine, string signature, object operation, IMethodCallMessage methodCall)
        {
            throw new NotSupportedException("Proxy method not allowed");            
        }
    }
}