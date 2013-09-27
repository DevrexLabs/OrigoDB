using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrigoDB.Core.Proxy;

namespace OrigoDB.Core.Test
{
    [Serializable]
    public class MyModel : Model
    {
        public MyModel()
        {
            AddChild(new MyChildModel());
        }
    }

    [Serializable]
    public class MyChildModel : Model
    {
        private int _greetingsReturned;

        public int Greetings
        {
            get { return _greetingsReturned; }
        }

        public String Greeting()
        {
            _greetingsReturned++;
            return "Hello!";
        }
    }

    [Serializable]
    public class MyChildModelCommand : Command<MyChildModel>
    {

        protected internal override void Execute(MyChildModel model)
        {
            Console.WriteLine("Hello");
        }
    }

    [Serializable]
    public class MyChildCommandWithResults : CommandWithResult<MyChildModel,int>
    {

        protected internal override int Execute(MyChildModel model)
        {
            return 42;
        }
    }

    [Serializable]
    public class MyChildQuery : Query<MyChildModel, int>
    {
        protected override int Execute(MyChildModel model)
        {
            return 42;
        }
    }
    

    [TestClass]
    public class ChildModelTests
    {

        [TestInitialize]
        public void Setup()
        {
            _engine = Engine.For<MyModel>(Guid.NewGuid().ToString());
        }

        [TestCleanup]
        public void TearDown()
        {
            Config.Engines.CloseAll();
        }



        [TestMethod]
        public void can_invoke_command_bound_to_child_model()
        {
            var cmd = new MyChildModelCommand();
            _engine.Execute(cmd);
        }

        [TestMethod]
        public void can_invoke_command_with_result_bound_to_child_model()
        {
            var cmd = new MyChildCommandWithResults();
            int result = _engine.Execute(cmd);
            Assert.AreEqual(42, result);
        }

        private IEngine<MyModel> _engine; 

        [TestMethod]
        public void can_invoke_query_bound_to_child_model()
        {

            var query = new MyChildQuery();
            int result = _engine.Execute(query);
            Assert.AreEqual(42,result);
        }

        [TestMethod]
        public void can_get_proxy_for_child_model()
        {
            var db = (MyModel) new ModelProxy<MyModel>(_engine).GetTransparentProxy();
            var childDb = db.ChildFor<MyChildModel>();
        }


        [TestMethod]
        public void can_get_proxy_for_child_model_2()
        {
            var child = _engine.GetProxy().ChildFor<MyChildModel>();
        }

        [TestMethod]
        public void method_call_for_childmodel_is_proxied()
        {
            var db = (MyModel)new ModelProxy<MyModel>(_engine).GetTransparentProxy();
            var childDb = db.ChildFor<MyChildModel>();
            childDb.Greeting();
            int actual = (_engine as LocalEngineClient<MyModel>).Execute((MyModel m) => m.ChildFor<MyChildModel>().Greetings);

            //assert Greeting was called on the correct model, not a clone
            Assert.AreEqual(1, actual);
        }


    }
}
