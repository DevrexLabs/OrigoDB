using System;

namespace OrigoDB.Core.Proxy
{
    /// <summary>
    /// Explicitly disallow when proxying, invocation will throw an Exception if called through the proxy
    /// </summary>
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property)]
    public class NoProxyAttribute : ProxyAttribute
    {
        public NoProxyAttribute()
        {
            Operation = OperationType.Disallowed;
        }
    }
}