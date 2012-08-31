using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    public static class Extensions
    {

        public static Int64 ParsePadded(this string number)
        {
            //Get rid of the leading zeros
            number = number.TrimStart('0');
            if (number == "") return 0;
            return Int32.Parse(number);

        }
    }
}
