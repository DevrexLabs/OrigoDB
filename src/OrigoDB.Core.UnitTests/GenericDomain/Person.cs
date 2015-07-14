using System;

namespace OrigoDB.Test.NUnit.GenericDomain
{
    [Serializable]
    public class Person : IDomainElement<Guid>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}