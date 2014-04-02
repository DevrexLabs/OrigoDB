using System;

namespace OrigoDB.Core.Proxy
{
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class QueryAttribute : ProxyAttribute
    {
        public QueryAttribute()
        {
            Operation = OperationType.Query;
        }
    }
}