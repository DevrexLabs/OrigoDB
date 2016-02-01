using System.Linq;
using NUnit.Framework;
using OrigoDB.Core;
using OrigoDB.Core.Modeling.Relational;
using OrigoDB.Core.Test;

namespace OrigoDB.Test.Models
{
    public class RelationalTestBase
    {
        protected RelationalModel _db;
        protected Engine<RelationalModel> _engine;
        protected Customer _aCustomer;
        protected IEntity[] _100Customers;

        [SetUp]
        public void Setup()
        {
            var config = new EngineConfiguration().ForIsolatedTest();
            _engine = ((LocalEngineClient<RelationalModel>)Engine.For<RelationalModel>(config)).Engine;
            _db = _engine.GetProxy();

            _aCustomer = new Customer
            {
                Address = new Address
                {
                    City = "Gotham",
                    ZipCode = "90240",
                    Street = "112 Mean street"
                },
                Name = "Homer Simpson"
            };

            _100Customers = Enumerable.Range(1, 1000)
                .Select(i => new Customer { Name = (i % 100).ToString() })
                .Cast<IEntity>().ToArray();

        }
        
    }
}