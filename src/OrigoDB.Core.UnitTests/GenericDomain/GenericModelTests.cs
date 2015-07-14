using System;
using System.Linq;
using NUnit.Framework;
using OrigoDB.Core;
using TC = OrigoDB.Test.NUnit.GenericDomain.TestContext;

namespace OrigoDB.Test.NUnit.GenericDomain
{
    [TestFixture]
    public class GenericModelTests
    {
        #region Person
        public void GivenAPerson()
        {
            var personId = Guid.NewGuid();
            TC.Bag.PersonId = personId;
            var person = new Person { Id = personId, FirstName = "John", LastName = "Rambo" };
            TC.Bag.Person = person;
        }

        public void GivenAGenericModelForPerson()
        {
            var genericModel = new GenericModel<Person, Guid>();
            TC.Bag.GenericModel = genericModel;
        }

        public void GivenAnEngineForPersonModel()
        {
            var engine = Engine.For<GenericModel<Person, Guid>>();
            TC.Bag.Engine = engine;
        }

        public void GivenAnInsertPersonCommand()
        {
            var entity = TC.Bag.Person as Person;
            TC.Bag.Entity = entity;
            var command = new InsertCommand<Person, Guid>(entity);
            TC.Bag.InsertCommand = command;
        }

        public void WhenExecutingTheInsertPersonCommandUsingThatEngine()
        {
            var engine = TC.Bag.Engine as IEngine<GenericModel<Person, Guid>>;
            var command = TC.Bag.InsertCommand as InsertCommand<Person, Guid>;
            engine.Execute(command);
        }

        public void ThenTheInsertedPersonShouldBeRetrievedByAQuery()
        {
            //Execute
            var personId = (Guid)TC.Bag.PersonId;
            var query = new GetByQuery<Person, Guid>(p => p.Id == personId);
            var engine = TC.Bag.Engine as IEngine<GenericModel<Person, Guid>>;
            var person = engine.Execute(query).SingleOrDefault();

            //Assert
            Assert.IsNotNull(person);
            Assert.That(person.FirstName == "John");
            Assert.That(person.LastName == "Rambo");
        }

        [Test]
        public void Engine_Should_Be_Able_To_Insert_And_Retrieve_a_Person_For_Generic_Domain()
        {
            GivenAPerson();
            GivenAGenericModelForPerson();
            GivenAnEngineForPersonModel();
            GivenAnInsertPersonCommand();
            WhenExecutingTheInsertPersonCommandUsingThatEngine();
            ThenTheInsertedPersonShouldBeRetrievedByAQuery();
        }
        #endregion

        #region Product
        public void GivenAProduct()
        {
            var productId = Guid.NewGuid();
            TC.Bag.ProductId = productId;
            var product = new Product { Id = productId, Name = "Coffee" };
            TC.Bag.Product = product;
        }

        public void GivenAGenericModelForThatProduct()
        {
            var genericModel = new GenericModel<Product, Guid>();
            TC.Bag.GenericModel = genericModel;
        }

        public void GivenAnEngineForProductModel()
        {
            var engine = Engine.For<GenericModel<Product, Guid>>();
            TC.Bag.Engine = engine;
        }

        public void GivenAnInsertProductCommand()
        {
            var entity = TC.Bag.Product as Product;
            TC.Bag.Entity = entity;
            var command = new InsertCommand<Product, Guid>(entity);
            TC.Bag.InsertCommand = command;
        }

        public void WhenExecutingTheInsertProductCommandUsingThatEngine()
        {
            var engine = TC.Bag.Engine as IEngine<GenericModel<Product, Guid>>;
            var command = TC.Bag.InsertCommand as InsertCommand<Product, Guid>;
            engine.Execute(command);
        }

        public void ThenTheInsertedProductShouldBeRetrievedByAQuery()
        {
            //Execute
            var productId = (Guid)TC.Bag.ProductId;
            var query = new GetByQuery<Product, Guid>(p => p.Id == productId);
            var engine = TC.Bag.Engine as IEngine<GenericModel<Product, Guid>>;
            var product = engine.Execute(query).SingleOrDefault();

            //Assert
            Assert.IsNotNull(product);
            Assert.That(product.Name == "Coffee");
        }

        [Test]
        public void Engine_Should_Be_Able_To_Insert_And_Retrieve_a_Product_For_Generic_Domain()
        {
            GivenAProduct();
            GivenAGenericModelForThatProduct();
            GivenAnEngineForProductModel();
            GivenAnInsertProductCommand();
            WhenExecutingTheInsertProductCommandUsingThatEngine();
            ThenTheInsertedProductShouldBeRetrievedByAQuery();
        }
        #endregion

        #region Order

        private void GivenOnOrder()
        {
            var orderId = Guid.NewGuid();
            TC.Bag.OrderId = orderId;
            var product = TC.Bag.Product as Product;
            var customer = TC.Bag.Person as Person;
            var order = new Order { Id = orderId, Product = product, Customer = customer, Date = DateTime.Now };
            TC.Bag.Order = order;
        }

        private void GivenAGenericModelForThatOrder()
        {
            var genericModel = new GenericModel<Order, Guid>();
            TC.Bag.GenericModel = genericModel;
        }
        private void GivenAnEngineForOrderModel()
        {
            var engine = Engine.For<GenericModel<Order, Guid>>();
            TC.Bag.Engine = engine;
        }
        private void GivenAnInsertOrderCommand()
        {
            var entity = TC.Bag.Order as Order;
            TC.Bag.Entity = entity;
            var command = new InsertCommand<Order, Guid>(entity);
            TC.Bag.InsertCommand = command;
        }
        private void WhenExecutingTheInsertOrderCommandUsingThatEngine()
        {
            var engine = TC.Bag.Engine as IEngine<GenericModel<Order, Guid>>;
            var command = TC.Bag.InsertCommand as InsertCommand<Order, Guid>;
            engine.Execute(command);
        }

        private void ThenTheInsertedOrderShouldBeRetrievedByAQuery()
        {
            //Execute
            var orderId = (Guid)TC.Bag.OrderId;
            var query = new GetByQuery<Order, Guid>(o => o.Id == orderId);
            var engine = TC.Bag.Engine as IEngine<GenericModel<Order, Guid>>;
            var order = engine.Execute(query).SingleOrDefault();

            //Assert
            Assert.IsNotNull(order);
            Assert.That(order.Id == orderId);
        }

        [Test]
        public void Engine_Should_Be_Able_To_Insert_And_Retrieve_an_Order_For_Generic_Domain()
        {
            GivenAPerson();
            GivenAProduct();
            GivenOnOrder();
            GivenAGenericModelForThatOrder();
            GivenAnEngineForOrderModel();
            GivenAnInsertOrderCommand();
            WhenExecutingTheInsertOrderCommandUsingThatEngine();
            ThenTheInsertedOrderShouldBeRetrievedByAQuery();
        }
        private void Given4OrdersForThatPerson()
        {
            var engine = TC.Bag.Engine as IEngine<GenericModel<Order, Guid>>;
            for (var i = 0; i < 3; i++)
            {
                var orderId = Guid.NewGuid();
                var product = TC.Bag.Product as Product;
                var customer = TC.Bag.Person as Person;
                var order = new Order { Id = orderId, Product = product, Customer = customer, Date = DateTime.Now };

                var command = new InsertCommand<Order, Guid>(order);
                engine.Execute(command);
            }
        }

        private void ThenAllOrdersForThatPersonShouldBeRetrieved()
        {
            var person = TC.Bag.Person as Person;
            var query = new GetByQuery<Order, Guid>(o => o.Customer.Id == person.Id);
            var engine = TC.Bag.Engine as IEngine<GenericModel<Order, Guid>>;
            var orders = engine.Execute(query);
            Assert.That(orders.Any());
        }
        [Test]
        public void Engine_Should_Be_Able_To_Retrieve_All_Orders_For_a_Given_Person()
        {
            GivenAPerson();
            GivenAProduct();
            GivenAnEngineForOrderModel();
            Given4OrdersForThatPerson();
            ThenAllOrdersForThatPersonShouldBeRetrieved();
        }

        #endregion
    }
}