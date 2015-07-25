using System;

namespace OrigoDB.Core.Test
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
