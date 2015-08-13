using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{

    [Serializable]
    public class MyModel<T> : Model
    {
        internal List<T> Items = new List<T>();

        public void Add(T item)
        {
            Items.Add(item);
        }

        public T ItemAt(int index)
        {
            return Items[index];
        }
    }

    [Serializable]
    public class MyQuery<T> : Command<MyModel<T>,int>
    {

        public override int Execute(MyModel<T> model)
        {
            return model.Items.Count;
        }
    }

    [TestFixture]
    public class GenericModelTest
    {
        [Test]
        public void Test()
        {
            var config = new EngineConfiguration().ForIsolatedTest();
            var engine = Engine.For<MyModel<String>>(config);
            var db = engine.GetProxy();
            db.Add("Fish");
            db.Add("Dog");
            var actual = db.ItemAt(1);
            Assert.AreEqual(actual, "Dog");
            int count = engine.Execute(new MyQuery<string>());
            Assert.AreEqual(2,count);
        }
    }
}