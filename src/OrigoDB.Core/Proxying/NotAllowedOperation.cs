using System;
using System.Reflection;
using OrigoDB.Core;

namespace Proxying
{
    internal class NotAllowedOperation<T> : OperationInfo<T> where T : Model
    {
        public NotAllowedOperation(MethodInfo methodInfo, OperationAttribute operationAttribute) 
            : base(methodInfo, operationAttribute)
        {
        }

        protected override object Execute(IEngine<T> engine, string signature, object operation, object[] args)
        {
            throw new Exception("Proxy method not allowed");
        }
    }
}