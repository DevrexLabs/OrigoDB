using System;

namespace Proxying
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class QueryAttribute : OperationAttribute
    {
        public static readonly OperationAttribute Default = new QueryAttribute();

        public QueryAttribute()
        {
            Type = OperationType.Query;
        }
    }
}