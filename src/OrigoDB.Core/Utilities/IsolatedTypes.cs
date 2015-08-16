using System;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core
{

    /// <summary>
    /// Commands and transaction return values do not have to be cloned if they are known to be isolated.
    /// Immutable types, enums and primitive types are immutable, thus isolated.
    /// </summary>
    public static class IsolatedTypes
    {

        private static readonly HashSet<Type> TheTypes = new HashSet<Type>()
        {
            typeof (string),
            typeof (object)
        };

        internal static void Add(Type type)
        {
            lock (TheTypes)
            {
                TheTypes.Add(type);
            }
        }

        internal static void AddRange(ISet<Type> isolatedTypes)
        {
                TheTypes.UnionWith(isolatedTypes);    
        }

        internal static bool Contains(Type type)
        {
                return TheTypes.Contains(type); 
        }

        /// <summary>
        /// Return true if a type is *known* to be isolated
        /// </summary>
        public static bool IsIsolated(this Type type)
        {
            return type.IsEnum 
                || type.IsPrimitive 
                || type.HasImmutableAttribute() 
                || Contains(type);
        }

        public static bool HasImmutableAttribute(this Type type)
        {
            return type
                .GetCustomAttributes(typeof (ImmutableAttribute), inherit: false).
                Any();
        }
    }
}