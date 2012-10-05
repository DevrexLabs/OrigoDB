using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LiveDomain.Core.Proxy
{

    internal abstract class ProxyMethodMap
    {
        private readonly static Dictionary<Type, ProxyMethodMap> _proxyMethodMaps 
            = new Dictionary<Type, ProxyMethodMap>();

        internal static ProxyMethodMap<M> GetProxyMethodMap<M>() where M : Model
        {
            Type modelType = typeof(M);
            ProxyMethodMap proxyMethodMap;
            if (!_proxyMethodMaps.TryGetValue(modelType, out proxyMethodMap))
            {
                proxyMethodMap = new ProxyMethodMap<M>();
                _proxyMethodMaps.Add(modelType, proxyMethodMap);
            }
            return (ProxyMethodMap<M>) proxyMethodMap;
        }
    }

	internal class ProxyMethodMap<M> : ProxyMethodMap where M : Model
	{
        //methods by name
	    private readonly Dictionary<string, ProxyMethodInfo> _proxyMethodInfoMap;

	    internal ProxyMethodMap()
	    {
            _proxyMethodInfoMap = new Dictionary<string, ProxyMethodInfo>();
            foreach(var methodInfo in typeof(M).GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var proxyMethodAttribute = GetProxyMethodAttribute(methodInfo);
                string methodName = methodInfo.Name;
                var proxyMethod = new ProxyMethodInfo(methodInfo, proxyMethodAttribute, "");
                _proxyMethodInfoMap.Add(methodName, proxyMethod);
            }
	    }

        private ProxyMethodAttribute GetProxyMethodAttribute(MethodInfo methodInfo)
        {
            var attribute = (ProxyMethodAttribute)methodInfo
                .GetCustomAttributes(typeof(ProxyMethodAttribute), false)
                .FirstOrDefault() ?? new ProxyMethodAttribute();

            if (attribute.TransactionType == TransactionType.Unspecified)
            {
                attribute.TransactionType = GetTransactionTypeFromMethodInfo(methodInfo);
            }
            return attribute;
        }

        private TransactionType GetTransactionTypeFromMethodInfo(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(void)
                ? TransactionType.Command
                : TransactionType.Query;
        }


        internal ProxyMethodInfo GetProxyMethodInfo(string methodName)
        {
            return _proxyMethodInfoMap[methodName];
        }
    }
}