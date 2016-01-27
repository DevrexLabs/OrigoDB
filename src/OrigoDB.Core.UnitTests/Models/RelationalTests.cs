using System;
using NUnit.Framework;
using OrigoDB.Core;
using OrigoDB.Core.Modeling.Relational;
using OrigoDB.Core.Test;

namespace OrigoDB.Test.Models
{
    [Serializable]
    class Customer : Entity
    {
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    [Serializable]
    public class Address
    {
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
    }

    [TestFixture]
    public class RelationalTests
    {
        [Test]
        public void SmokeTest()
        {
            var customer = new Customer
            {
                Address = new Address
                {
                    City = "Gotham",
                    ZipCode = "90240",
                    Street = "112 Mean street"
                },
                Name = "Homer Simpson"
            };
            var db = Db.For<RelationalModel>(new EngineConfiguration().ForIsolatedTest());
            db.Create<Customer>();
            db.Insert(customer);
            customer.Address.City = "Springfield";
            db.Update(customer);
            db.Delete(customer);
        }
    }
}