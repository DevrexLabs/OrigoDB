using System;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core
{

    /// <summary>
    /// Commands and transaction return values do not have to be cloned if they are known to be isolated.
    /// Immutable types, enums and primitive types are immutable, thus isolated.
    /// </summary>
    internal static class IsolatedReturnTypes
    {
        private static readonly HashSet<Type> TheTypes = new HashSet<Type>()
        {
            typeof (string),
            typeof (object),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(TimeZone),
            typeof(TimeZoneInfo),
            typeof(DateTimeOffset),
            typeof(Uri),
            typeof(Version)
        };

        internal static void Add(Type type)
        {
            TheTypes.Add(type);
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
        /// Return true if a type is *known* to be isolated when returned from a query or command
        /// </summary>
        public static bool IsIsolated(this Type type)
        {
            return type.IsEnum 
                || type.IsPrimitive 
                || type.HasImmutableAttribute() 
                || Contains(type);
        }

        internal static bool HasImmutableAttribute(this Type type)
        {
            return type.HasAttribute<ImmutableAttribute>();
        }

        private static bool HasAttribute<T>(this Type type)
        {
            return type
                .GetCustomAttributes(typeof(T), inherit: false).
                Any();
        }
    }
}