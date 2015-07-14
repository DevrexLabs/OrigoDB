using System;

namespace OrigoDB.Test.NUnit.GenericDomain
{
    [Serializable]
    public class Product : IDomainElement<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
