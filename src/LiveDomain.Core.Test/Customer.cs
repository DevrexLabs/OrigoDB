using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Test
{
    [Serializable]
    public class Customer : ICloneable
    {
        public string Name { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
