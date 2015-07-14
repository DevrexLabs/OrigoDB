using System;

namespace OrigoDB.Core.Utilities
{
    public static class Ensure
    {
       public static void NotNull(object param, string paramName)
        {
            if (param == null) throw new ArgumentNullException(paramName);
        }

        internal static void NotNullOrEmpty(string name, string paramName)
        {
            NotNull(name, paramName);
            if(String.IsNullOrWhiteSpace(name)) throw new ArgumentException(paramName);
        }

        internal static void That(bool assertion, string errorMessage)
        {
            if (!assertion) throw new ArgumentException(errorMessage);
        }
    }
}
