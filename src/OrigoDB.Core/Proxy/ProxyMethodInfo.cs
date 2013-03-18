using System.Reflection;

namespace OrigoDB.Core.Proxy
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
                return ProxyMethodAttribute.OperationType == OperationType.Command;
            }
        }

        public bool IsQuery
        {
            get
            {
                return ProxyMethodAttribute.OperationType == OperationType.Query;
            }
        }
    }
}