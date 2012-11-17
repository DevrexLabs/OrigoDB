using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Utilities
{
    public static class Ensure
    {

        public static void NotNull(object o, string param)
        {
            if (o == null) throw new ArgumentNullException(param);
        }
    }
}
