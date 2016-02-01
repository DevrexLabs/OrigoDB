using System;
using System.Linq;
using NUnit.Framework;
using OrigoDB.Core;
using OrigoDB.Core.Modeling.Relational;

namespace OrigoDB.Test.Models
{
    [TestFixture]
    public class RelationalTests : RelationalTestBase
    {


        [Test]
        public void Adhoc_lambda_query()
        {
            _db.Create<Customer>();
            _db._Insert(_100Customers);
            var customer = _engine.Execute(db => db.From<Customer>().FirstOrDefault(c => c.Name.StartsWith("42")));

            Assert.IsNotNull(customer);
            Assert.IsTrue(customer.Name.StartsWith("42"));

        }

        [Test]
        public void Query_class_smoke()
        {
            
            _db.Create<Customer>();
            _db._Insert(_100Customers);

            //execute query
            var result = _engine.Execute(new CustomersQuery("1"));
            Assert.IsTrue(result.All(s => s.StartsWith("1")));
        }

        [Test, ExpectedException(typeof(CommandAbortedException))]
        public void Insert_rejected_unless_type_exists()
        {
            _db._Insert(_aCustomer);
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
            Assert.AreEqual(0, _aCustomer.Version);
            _db.Insert(_aCustomer);
            Assert.AreEqual(1, _aCustomer.Version, "Version wasn't incremented");
        }

        [Test]
        public void Can_delete()
        {
            _db.Create<Customer>();
            _db.Insert(_aCustomer);
            _db.Delete(_aCustomer);
        }

        [Test]
        public void Can_delete_using_EntityKey()
        {
            _db.Create<Customer>();
            _db.Insert(_aCustomer);
            var key = _aCustomer.ToKey();
            _db.Delete(key);
        }

        [Test]
        public void Can_update()
        {
            const string name = "Donald T. Rump";
            _db.Create<Customer>();
            _db.Insert(_aCustomer);
            _aCustomer.Name = name;
            _db.Update(_aCustomer);
            Assert.AreEqual(2, _aCustomer.Version, "Version wan't incremented");
            Assert.AreEqual(name, _db.TryGetById<Customer>(_aCustomer.Id).Name);
        }

        [Test]
        public void Can_get_by_id()
        {
            _db.Create<Customer>();
            _db.Insert(_aCustomer);
            var actual = _db.TryGetById<Customer>(_aCustomer.Id);
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.Id, _aCustomer.Id);
            Assert.AreEqual(actual.GetType(), typeof(Customer));
            Assert.AreEqual(_aCustomer.Version, actual.Version, "version mistmatch after insert");
        }

        [Test]
        public void Create_can_be_called_multiple_times_and_returns_correct_value()
        {
            Assert.IsTrue(_db.Create<Customer>());
            Assert.IsFalse(_db.Create<Customer>());
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