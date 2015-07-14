using System;

namespace OrigoDB.Test.NUnit.GenericDomain
{
    [Serializable]
    public class Order : IDomainElement<Guid>
    {
        public Guid Id { get; set; }
        public Product Product { get; set; }
        public Person Customer { get; set; }
        public DateTime Date { get; set; }
    }
}