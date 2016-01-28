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
        private RelationalModel _db;
        private Customer _aCustomer;

        [SetUp]
        public void Setup()
        {
            var config = new EngineConfiguration().ForIsolatedTest();
            _db = Db.For<RelationalModel>(config);

            _aCustomer = new Customer{
                Address = new Address
                {
                    City = "Gotham",
                    ZipCode = "90240",
                    Street = "112 Mean street"
                },
                Name = "Homer Simpson"
            };
        }

        [Test, ExpectedException(typeof(CommandAbortedException))]
        public void Insert_rejected_unless_type_exists()
        {
            _db.Insert(_aCustomer);
        }

        [Test]
        public void Type_exists_when_created()
        {
            _db.Create<Customer>();
            Assert.IsTrue(_db.Exists<Customer>());
        }

        [Test]
        public void Type_does_not_exist_unless_created()
        {
            Assert.IsFalse(_db.Exists<Customer>());
        }

        [Test]
        public void Can_insert()
        {
            _db.Create<Customer>();
            _db.Insert(_aCustomer);
        }

        [Test]
        public void TryGetById_returns_null_when_not_found()
        {
            _db.Create<Customer>();
            var result = _db.TryGetById<Customer>(Guid.NewGuid());
            Assert.IsNull(result);
        }

        [Test, ExpectedException(typeof(CommandAbortedException))]
        public void GetById_throws_unless_type_exists()
        {
            _db.TryGetById<Customer>(Guid.NewGuid());
        }
    }
}