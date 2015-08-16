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

        protected override object Execute(IEngine<T> engine, string signature, object query, IMethodCallMessage methodCallMessage)
        {
            if (query == null)
            {
                var proxyQuery = new ProxyQuery<T>(signature, methodCallMessage.Args, methodCallMessage.MethodBase.GetGenericArguments());
                proxyQuery.ResultIsIsolated = ResultIsIsolated;
                query = proxyQuery;
            }
            return engine.Execute((Query)query);
        }
    }
}