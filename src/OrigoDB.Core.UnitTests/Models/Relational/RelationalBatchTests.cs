using System;
using System.Linq;
using NUnit.Framework;
using OrigoDB.Core.Modeling.Relational;

namespace OrigoDB.Test.Models
{
    [TestFixture]

    public class RelationalBatchTests : RelationalTestBase
    {
        [Test]
        public void Multiple_operations()
        {
            Batch batch = new Batch();
            batch.Insert(_aCustomer);


            _db.Create<Customer>();

            //insert will bump version of local entity to 1
            _db.Insert(_100Customers[0]);

            //update some field and add to the batches updates
            ((Customer) _100Customers[0]).Name = "Donald Trump";
            batch.Update(_100Customers[0]);


            _db.Insert(_100Customers[1]);   //will bump version of local entity after insert
            batch.Delete(_100Customers[1]); //

            //should be 2 customers before the batch
            Assert.AreEqual(2, _engine.Execute(db => db.From<Customer>().Count()));
            
            _db.Execute(batch);

            //2 customers after batch (1 was deleted, 1 was inserted)
            Assert.AreEqual(2, _engine.Execute(db => db.From<Customer>().Count()));

            //make sure update was persisted
            Assert.AreEqual("Donald Trump", _engine.Execute(db => db.TryGetById<Customer>(_100Customers[0].Id).Name));

            //make sure deleted does not exists
            Assert.IsNull(_db.TryGetById<Customer>(_100Customers[1].Id));
            
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Cant_add_duplicate_keys_to_batch()
        {
            Batch target = new Batch();
            target.Insert(_aCustomer);
            target.Insert(_aCustomer);
        }

        [Test, ExpectedException(typeof(OptimisticConcurrencyException))]
        public void Simple_concurrency_conflict()
        {
            Batch b1 = new Batch();
            Batch b2 = new Batch();

            b1.Insert(_aCustomer);
            b2.Insert(_aCustomer);

            _db.Create<Customer>();
            _db.Execute(b1);
            try
            {
                _db.Execute(b2);
            }
            catch (Exception ex)
            {
                var conflicts = ((OptimisticConcurrencyException)ex).Conflicts;
                CollectionAssert.IsEmpty(conflicts.Updates);
                CollectionAssert.IsEmpty(conflicts.Deletes);
                Assert.AreEqual(1, conflicts.Inserts.Count, "Expected single insert conflict");
                Assert.AreEqual(1, conflicts.Inserts.First().Version);
                throw;
            }

        }
    }
}