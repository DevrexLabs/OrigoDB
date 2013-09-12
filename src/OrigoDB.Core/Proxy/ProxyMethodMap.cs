using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrigoDB.Core.Proxy
{

    internal class ProxyMethodMap
    {
        private readonly static Dictionary<Type, ProxyMethodMap> _proxyMethodMaps
            = new Dictionary<Type, ProxyMethodMap>();

        internal static ProxyMethodMap MapFor<T>()
        {
            return MapFor(typeof(T));
        }

        internal static ProxyMethodMap MapFor(Type modelType)
        {
            ProxyMethodMap proxyMethodMap;
            if (!_proxyMethodMaps.TryGetValue(modelType, out proxyMethodMap))
            {
                proxyMethodMap = new ProxyMethodMap(modelType);
                _proxyMethodMaps.Add(modelType, proxyMethodMap);
            }
            return proxyMethodMap;
        }

        //methods by name
        private readonly Dictionary<string, ProxyMethodInfo> _proxyMethodInfoMap;

        internal ProxyMethodMap(Type modelType)
        {
            _proxyMethodInfoMap = new Dictionary<string, ProxyMethodInfo>();
            foreach (var methodInfo in modelType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
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

            if (attribute.OperationType == OperationType.Unspecified)
            {
                attribute.OperationType = GetOperationTypeFromMethodInfo(methodInfo);
            }
            return attribute;
        }

        private OperationType GetOperationTypeFromMethodInfo(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(void)
                ? OperationType.Command
                : OperationType.Query;
        }


        internal ProxyMethodInfo GetProxyMethodInfo(string methodName)
        {
            return _proxyMethodInfoMap[methodName];
        }
    }
}