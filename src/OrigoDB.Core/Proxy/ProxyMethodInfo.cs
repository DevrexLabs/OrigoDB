using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace OrigoDB.Core.Proxy
{

    internal sealed class QueryInfo<T> : OperationInfo<T> where T:Model
    {
        public QueryInfo(MethodInfo methodInfo, ProxyAttribute attribute)
            : base(methodInfo, attribute)
        {

        }

        protected override object Execute(IEngine<T> engine, string signature, object operation, object[] args)
        {
            var query = (Query) operation;
            query = query ?? new ProxyQuery<T>(signature, args);
            query.ResultIsSafe = !ProxyAttribute.CloneResult;
            return engine.Execute(query);
        }
    }

    internal sealed class CommandInfo<T> : OperationInfo<T> where T:Model
    {
        public CommandInfo(MethodInfo methodInfo, ProxyAttribute attribute)
            : base(methodInfo, attribute)
        {

        }

        protected override object Execute(IEngine<T> engine, string signature, object operation, object[] args)
        {
            var command = (Command) operation;
            command = command ?? new ProxyCommand<T>(signature, args);
            //todo: command.ResultIsSafe = !ProxyAttribute.CloneResult;
            return engine.Execute(command);
        }
    }

    internal abstract class OperationInfo<T> where T : Model
    {

        public readonly MethodInfo MethodInfo;
        public readonly ProxyAttribute ProxyAttribute;

        protected OperationInfo(MethodInfo methodInfo, ProxyAttribute proxyMethodAttribute)
        {
            MethodInfo = methodInfo;
            ProxyAttribute = proxyMethodAttribute;
        }

        public bool IsAllowed
        {
            get
            {
                return ProxyAttribute.Operation != OperationType.Disallowed;
            }
        }

        protected bool IsMapped
        {
            get { return ProxyAttribute.MapTo != null; }
        }

        private object GetMappedOperation(IMethodCallMessage callMessage)
        {
            var mapTo = ProxyAttribute.MapTo;
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

        public static OperationInfo<T> Create(MethodInfo methodInfo, ProxyAttribute proxyAttribute)
        {
            if (proxyAttribute.Operation == OperationType.Command) return new CommandInfo<T>(methodInfo, proxyAttribute);
            if (proxyAttribute.Operation == OperationType.Query) return new QueryInfo<T>(methodInfo,proxyAttribute);
            throw new ArgumentException("Expected operation Command or Query", "proxyAttribute");
        }
    }
}