using System.Reflection;

namespace OrigoDB.Core.Proxy
{

    internal class ProxyMethodInfo
    {
        public readonly MethodInfo MethodInfo;

        public readonly ProxyAttribute ProxyAttribute;

        public ProxyMethodInfo(MethodInfo methodInfo, ProxyAttribute proxyMethodAttribute)
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

        public bool IsCommand
        {
            get
            {
                return ProxyAttribute.Operation == OperationType.Command;
            }
        }

        public bool IsQuery
        {
            get
            {
                return ProxyAttribute.Operation == OperationType.Query;
            }
        }
    }
}