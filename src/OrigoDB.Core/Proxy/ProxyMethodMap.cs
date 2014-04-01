using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrigoDB.Core.Proxy
{

    /// <summary>
    /// Map method names to MethodInfos and user/system metadata
    /// </summary>
    internal class ProxyMethodMap
    {

        private readonly static Dictionary<Type, ProxyMethodMap> _proxyMethodMaps
            = new Dictionary<Type, ProxyMethodMap>();


        private readonly Type modelType;
        
        /// <summary>
        /// Proxied methods by name
        /// </summary>
        private readonly Dictionary<string, ProxyMethodInfo> _proxyMethodInfoMap;


        internal static ProxyMethodMap MapFor<T>()
        {
            return MapFor(typeof(T));
        }

        internal static ProxyMethodMap MapFor(Type modelType)
        {
            ProxyMethodMap proxyMethodMap;
            if (!_proxyMethodMaps.TryGetValue(modelType, out proxyMethodMap))
            {
                proxyMethodMap = ProxyMethodMap.Create(modelType);
                _proxyMethodMaps.Add(modelType, proxyMethodMap);
            }
            return proxyMethodMap;
        }


        internal static ProxyMethodMap Create(Type modelType)
        {
            var result = new ProxyMethodMap(modelType);
            result.Build();
            return result;
        }

        internal void Build()
        {
            foreach (var methodInfo in modelType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var proxyMethodAttribute = GetProxyMethodAttribute(methodInfo);
                string methodName = methodInfo.Name;
                var proxyMethod = new ProxyMethodInfo(methodInfo, proxyMethodAttribute);
                _proxyMethodInfoMap.Add(methodName, proxyMethod);
            }

        }



        internal ProxyMethodMap(Type type)
        {
            _proxyMethodInfoMap = new Dictionary<string, ProxyMethodInfo>();
            modelType = type;
        }

        private ProxyAttribute GetProxyMethodAttribute(MethodInfo methodInfo)
        {
            var attribute = (ProxyAttribute)methodInfo
                .GetCustomAttributes(typeof(ProxyAttribute), true)
                .FirstOrDefault();

            if (attribute == null)
            {
                var temp = methodInfo.GetCustomAttributes(typeof (NoProxyAttribute), true).FirstOrDefault();
                if (temp != null) attribute = new ProxyAttribute{Operation = OperationType.Disallowed};
            }

            if (attribute == null)
            {
                attribute = DeriveAttributeFromMethodInfo(methodInfo);
            }
            return attribute;
        }

        private ProxyAttribute DeriveAttributeFromMethodInfo(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(void)
                ? (ProxyAttribute) new CommandAttribute()
                : new QueryAttribute();
        }

        internal ProxyMethodInfo GetProxyMethodInfo(string methodName)
        {
            return _proxyMethodInfoMap[methodName];
        }
    }
}