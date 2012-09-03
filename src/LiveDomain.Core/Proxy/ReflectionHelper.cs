using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LiveDomain.Core.Proxy
{
	public static class ReflectionHelper
	{
		static Dictionary<Type,MethodInfo[]> _typeInformation = new Dictionary<Type, MethodInfo[]>(); 

		public static MethodInfo ResolveMethod(Type type, string methodName)
		{
			if (!_typeInformation.ContainsKey(type))
				_typeInformation[type] = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

			return _typeInformation[type].First(i => String.Compare(i.Name, methodName, StringComparison.OrdinalIgnoreCase) == 0);
		}
	}
}