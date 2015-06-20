using System.Reflection;
using OrigoDB.Core;

namespace Proxying
{
    internal sealed class QueryInfo<T> : OperationInfo<T> where T:Model
    {
        public QueryInfo(MethodInfo methodInfo, OperationAttribute attribute)
            : base(methodInfo, attribute)
        {

        }

        protected override object Execute(IEngine<T> engine, string signature, object operation, object[] args)
        {
            var query = (Query) operation;
            query = query ?? new ProxyQuery<T>(signature, args);
            query.ResultIsSafe = !OperationAttribute.CloneResult;
            return engine.Execute(query);
        }
    }
}