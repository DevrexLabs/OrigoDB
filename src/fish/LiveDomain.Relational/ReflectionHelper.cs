using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Relational.Extensions
{
    public static class ReflectionHelper
    {
        public static bool Implements(this Type type, Type @interface)
        {
            return type.GetInterfaces().Any(t => t.FullName == @interface.FullName);
        }

        public static bool Inherits(this Type type, Type @class)
        {
            return type.IsSubclassOf(@class);
        }
    }
}
