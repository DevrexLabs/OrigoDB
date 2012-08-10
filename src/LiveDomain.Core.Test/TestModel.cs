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
        
        protected internal override void JournalRestored()
        {
            OnLoadExecuted = true;
        }

    }


    public class GetNumberOfCommandsExecutedQuery : Query<TestModel, int>
    {
        protected override int Execute(TestModel model)
        {
            return model.CommandsExecuted;
        }
    }

    [Serializable]
    public class TestCommandWithoutResult : Command<TestModel>
    {
        protected internal override void Execute(TestModel model)
        {
            model.CommandsExecuted++;
        }
    }

    [Serializable]
    public class TestCommandWithResult : CommandWithResult<TestModel,int>
    {
        public byte[] Payload { get; set; }
        public bool ThrowInPrepare { get; set; }
        public bool ThrowExceptionInExecute { get; set; }
        public bool ThrowCommandFailedExceptionFromExecute { get; set; }

        protected internal override void Prepare(TestModel model)
        {
            if (ThrowInPrepare)
            {
                throw new Exception();
            }
        }
        protected internal override int Execute(TestModel model)
        {
            if (ThrowCommandFailedExceptionFromExecute)
            {
                throw new CommandAbortedException();
            }
            if (ThrowExceptionInExecute)
            {
                throw new Exception();
            }
            return ++model.CommandsExecuted;
        }

    }
}
