using System;
using System.Linq;
using NUnit.Framework;
using Proxying;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class ProxyOverloadingTests
    {
        private ModelWithOverloads _db;

        [SetUp]
        public void Setup()
        {
            var config = EngineConfiguration.Create().ForIsolatedTest();
            _db = Db.For<ModelWithOverloads>(config);

        }

        [Test]
        public void CanCallNoArgMethod()
        {
            _db.Meth();
           Assert.AreEqual(_db.GetCalls(), 1);
        }

        [Test]
        public void CanCallOverloadWithAnArgument()
        {
            var inc = _db.Meth(42);
            Assert.AreEqual(43, inc);
        }

        [Test]
        public void CanCallWithParams()
        {
            
            var numbers = new[] {1, 2, 3, 4, 5};
            var sum = numbers.Sum();
            var result = _db.Meth(1,2,3,4,5);
            Assert.AreEqual(sum, result);
        }

        [Test]
        public void CanCallUsingNamedArgs()
        {
            var result = _db.Inc(with: 100, number: 200);
            Assert.AreEqual(300, result);
        }

        [Test]
        public void CanCallWithArrayAsParams()
        {
            var numbers = new[] { 1, 2, 3, 4, 5 };
            var sum = numbers.Sum();
            var result = _db.Meth(numbers);
            Assert.AreEqual(sum,result);
        }

        [Test]
        public void CanHandleOptionalArgs()
        {
            var result = _db.Inc(20);
            Assert.AreEqual(21, result);

            result = _db.Inc(20, 5);
            Assert.AreEqual(25,result);
        }


        /// <summary>
        /// It should not be possible to use ref or out args
        /// </summary>
        [Test()]
        [ExpectedException(typeof(Exception))]
        public void RefArgsNotAllowed()
        {
            Db.For<ModelWithRefArg>();
        }

        [Test()]
        [ExpectedException(typeof(Exception))]
        public void OutArgsNotAllowed()
        {
            Db.For<ModelWithOutArg>();
        }


        [Serializable]
        private class ModelWithOutArg : Model
        {
            public void Method(out int a)
            {
                a = 42;
            }
        }

        [Serializable]
        private class ModelWithRefArg : Model
        {
            public void Method(ref int a)
            {
                a = 42;
            }
        }
    }
}