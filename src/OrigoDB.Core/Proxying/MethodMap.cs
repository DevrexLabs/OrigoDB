using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OrigoDB.Core;

namespace Proxying
{

    internal abstract class MethodMap
    {
        private static readonly Dictionary<Type, MethodMap> MethodMaps
            = new Dictionary<Type, MethodMap>();

        internal static MethodMap<T> MapFor<T>() where T : Model
        {
            Type type = typeof(T);
            MethodMap methodMap;
            if (!MethodMaps.TryGetValue(type, out methodMap))
            {
                methodMap = MethodMap<T>.Create(type);
                MethodMaps.Add(type, methodMap);
            }
            return (MethodMap<T>)methodMap;
        } 
    }

    /// <summary>
    /// Map method signatures to MethodInfo
    /// </summary>
    internal sealed class MethodMap<T> : MethodMap where T:Model
    {
     
        /// <summary>
        /// Proxied methods by signature
        /// </summary>
        private readonly Dictionary<string, OperationInfo<T>> _theMap;


        internal static MethodMap<T> Create(Type modelType)
        {
            var methodMap = new Dictionary<string, OperationInfo<T>>();
            foreach (var methodInfo in modelType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                Validate(methodInfo);

                var operationAttribute = GetOperationAttribute(methodInfo);
                string methodName = methodInfo.Name;
                var operationInfo = OperationInfo<T>.Create(methodInfo, operationAttribute);

                //For backwards compatibility when overloads were not supported
                //Only name was used. Overloads were introduced with v 0.18.0
                if (!methodMap.ContainsKey(methodName)) methodMap.Add(methodName, operationInfo);
                
                //use a unique signature based on the method name and argument types
                var signature = methodInfo.ToString();
                methodMap.Add(signature, operationInfo);
            }

            var result = new MethodMap<T>(methodMap);
            return result;
        }


        private static void Validate(MethodInfo methodInfo)
        {
            if ( HasRefArg(methodInfo))
            {
                throw new Exception("ref/out parameters not supported");
            }
        }

        
        internal static Boolean HasRefArg(MethodInfo methodInfo)
        {
            return methodInfo
                .GetParameters()
                .Any(p => p.ParameterType.IsByRef || p.IsOut);
        }


        internal MethodMap(Dictionary<string, OperationInfo<T>> methodMap)
        {
            _theMap = methodMap;
        }

        private static OperationAttribute GetOperationAttribute(MethodInfo methodInfo)
        {
            //If there is an explicit attribute present, return it
            var attribute = (OperationAttribute)methodInfo
                .GetCustomAttributes(typeof(OperationAttribute), inherit:true)
                .FirstOrDefault();
            if (attribute != null) return attribute;

            var temp = methodInfo.GetCustomAttributes(typeof (NoProxyAttribute), true).FirstOrDefault();
            if (temp != null) attribute = new OperationAttribute{Type = OperationType.Disallowed};

            return attribute ?? GetDefaultOperationAttribute(methodInfo);
        }

        /// <summary>
        /// Void methods are considered as commands, methods with return values are considered queries.
        /// </summary>
        private static OperationAttribute GetDefaultOperationAttribute(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(void)
                ? CommandAttribute.Default
                : QueryAttribute.Default;
        }

        internal OperationInfo<T> GetOperationInfo(string signature)
        {
            return _theMap[signature];
        }
    }
}