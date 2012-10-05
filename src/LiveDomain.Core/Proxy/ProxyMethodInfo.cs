using System.Reflection;

namespace LiveDomain.Core.Proxy
{

    internal class ProxyMethodInfo
    {
        public readonly MethodInfo MethodInfo;
        public readonly ProxyMethodAttribute ProxyMethodAttribute;

        public ProxyMethodInfo(MethodInfo methodInfo, ProxyMethodAttribute proxyMethodAttribute, string methodName)
        {
            MethodInfo = methodInfo;
            ProxyMethodAttribute = proxyMethodAttribute;
        }

        public bool IsCommand
        {
            get
            {
                return ProxyMethodAttribute.TransactionType == TransactionType.Command;
            }
        }

        public bool IsQuery
        {
            get
            {
                return ProxyMethodAttribute.TransactionType == TransactionType.Query;
            }
        }
    }
}