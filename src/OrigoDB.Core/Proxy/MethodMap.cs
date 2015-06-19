using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Proxy
{

    /// <summary>
    /// Map method signatures to MethodInfo
    /// </summary>
    internal class MethodMap
    {

        /// <summary>
        /// A cache
        /// </summary>
        private static readonly Dictionary<Type, MethodMap> MethodMaps
            = new Dictionary<Type, MethodMap>();


        
        /// <summary>
        /// Proxied methods by signature
        /// </summary>
        private readonly Dictionary<string, ProxyMethodInfo> _theMap;


        internal static MethodMap MapFor<T>()
        {
            return MapFor(typeof(T));
        }

        internal static MethodMap MapFor(Type modelType)
        {
            MethodMap methodMap;
            if (!MethodMaps.TryGetValue(modelType, out methodMap))
            {
                methodMap = Create(modelType);
                MethodMaps.Add(modelType, methodMap);
            }
            return methodMap;
        }


        internal static MethodMap Create(Type modelType)
        {
            var methodMap = new Dictionary<string, ProxyMethodInfo>();
            foreach (var methodInfo in modelType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                Validate(methodInfo);

                var proxyMethodAttribute = GetProxyMethodAttribute(methodInfo);
                string methodName = methodInfo.Name;
                var proxyMethod = new ProxyMethodInfo(methodInfo, proxyMethodAttribute);

                //For backwards compatibility when overloads were not supported
                //Only name was used. Overloads were introduced with v 0.18.0
                if (!methodMap.ContainsKey(methodName)) methodMap.Add(methodName, proxyMethod);
                
                //use a unique signature based on the method name and arguments
                var signature = methodInfo.ToString();
                methodMap.Add(signature, proxyMethod);
            }

            var result = new MethodMap(methodMap);
            return result;
        }


        private static void Validate(MethodInfo methodInfo)
        {
            if ( HasRefArg(methodInfo) || HasOutArg(methodInfo))
            {
                throw new Exception("ref/out parameters not supported");
            }
        }

        internal static Boolean HasOutArg(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters().Any(p => p.IsOut);
        }

        internal static Boolean HasRefArg(MethodInfo methodInfo)
        {
            const string pattern = " ByRef[,)]";
            return Regex.IsMatch(methodInfo.ToString(), pattern);
        }


        internal MethodMap(Dictionary<string, ProxyMethodInfo> methodMap)
        {
            _theMap = methodMap;
        }

        private static ProxyAttribute GetProxyMethodAttribute(MethodInfo methodInfo)
        {
            //If there is an explicit attribute present, return it
            var attribute = (ProxyAttribute)methodInfo
                .GetCustomAttributes(typeof(ProxyAttribute), inherit:true)
                .FirstOrDefault();
            if (attribute != null) return attribute;

            var temp = methodInfo.GetCustomAttributes(typeof (NoProxyAttribute), true).FirstOrDefault();
            if (temp != null) attribute = new ProxyAttribute{Operation = OperationType.Disallowed};

            return attribute ?? DeriveAttributeFromMethodInfo(methodInfo);
        }

        /// <summary>
        /// Void methods are considered as commands, methods with return values are considered queries.
        /// </summary>
        private static ProxyAttribute DeriveAttributeFromMethodInfo(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(void)
                ? (ProxyAttribute) new CommandAttribute()
                : new QueryAttribute();
        }

        internal ProxyMethodInfo GetProxyMethodInfo(string signature)
        {
            return _theMap[signature];
        }
    }
}