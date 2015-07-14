using System;

namespace OrigoDB.Test.Common
{
    [Serializable]
    public class Entity
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
    }
}
