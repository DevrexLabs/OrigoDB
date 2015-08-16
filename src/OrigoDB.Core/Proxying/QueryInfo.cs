using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace OrigoDB.Core.Proxying
{
    internal sealed class QueryInfo<T> : OperationInfo<T> where T:Model
    {
        public QueryInfo(MethodInfo methodInfo, OperationAttribute attribute)
            : base(methodInfo, attribute)
        {

        }

        protected override object Execute(IEngine<T> engine, string signature, object operation, IMethodCallMessage methodCallMessage)
        {
            var query = (Query) operation;
            query = query ?? new ProxyQuery<T>(signature, methodCallMessage.Args, methodCallMessage.MethodBase.GetGenericArguments());
            query.ResultIsIsolated = OperationAttribute.ResultIsIsolated;
            return engine.Execute(query);
        }
    }
}