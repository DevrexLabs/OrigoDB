using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Utilities
{
    public static class Ensure
    {

        public static void NotNull(object param, string paramName)
        {
            if (param == null) throw new ArgumentNullException(paramName);
        }

        internal static void NotNullOrEmpty(string name, string paramName)
        {
            if(String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(paramName);
        }
    }
}
