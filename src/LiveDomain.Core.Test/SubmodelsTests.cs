using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiveDomain.Core.Test
{
    [Serializable]
    internal class MyModel : Model
    {
        public MyModel()
        {
            AddChildModel(new MyChildModel());
        }
    }

    [Serializable]
    class MyChildModel : Model
    {
        
    }

    [Serializable]
    class MyChildModelCommand : Command<MyChildModel>
    {

        protected internal override void Execute(MyChildModel model)
        {
            Console.WriteLine("Hello");
        }
    }

    [Serializable]
    class MyChildCommandWithResults : CommandWithResult<MyChildModel,int>
    {

        protected internal override int Execute(MyChildModel model)
        {
            return 42;
        }
    }

    [Serializable]
    class MyChildQuery : Query<MyChildModel, int>
    {
        protected override int Execute(MyChildModel m)
        {
            return 42;
        }
    }
    

    [TestClass]
    public class SubmodelsTests
    {
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

        IEngine<MyModel> _engine = Engine.For<MyModel>();

        [TestMethod]
        public void can_invoke_query_bound_to_child_model()
        {

            var query = new MyChildQuery();
            int result = _engine.Execute(query);
            Assert.AreEqual(42,result);
        }


    }
}
