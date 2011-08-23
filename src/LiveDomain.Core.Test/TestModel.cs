using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Test
{
    [Serializable]
    public class TestModel : Model
    {
        public int CommandsExecuted { get; set; }

        public bool OnLoadExecuted { get; private set; }
        protected override void OnLoad()
        {
            OnLoadExecuted = true;
        }

    }


    public class GetNumberOfCommandsExecutedQuery : Query<TestModel, int>
    {
        public override int Execute(TestModel model)
        {
            return model.CommandsExecuted;
        }
    }

    [Serializable]
    public class TestCommand : CommandWithResult<TestModel,int>
    {
        public bool ThrowInPrepare { get; set; }
        public bool ThrowExceptionInExecute { get; set; }
        public bool ThrowCommandFailedExceptionFromExecute { get; set; }

        protected override void Prepare(TestModel model)
        {
            if (ThrowInPrepare)
            {
                throw new Exception();
            }
        }
        protected override int Execute(TestModel model)
        {
            if (ThrowCommandFailedExceptionFromExecute)
            {
                throw new CommandFailedException();
            }
            if (ThrowExceptionInExecute)
            {
                throw new Exception();
            }
            return ++model.CommandsExecuted;
        }

    }
}
