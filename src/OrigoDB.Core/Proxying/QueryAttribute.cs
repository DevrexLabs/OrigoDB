using System;

namespace Proxying
{
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class QueryAttribute : OperationAttribute
    {
        public static readonly OperationAttribute Default = new QueryAttribute();

        public QueryAttribute()
        {
            Type = OperationType.Query;
        }
    }
}