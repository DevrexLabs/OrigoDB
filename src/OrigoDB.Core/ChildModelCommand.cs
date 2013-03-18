using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{
    [Serializable]
    internal class ChildModelCommand<M, C> : Command<M>
        where M : Model
        where C : Model
    {
        private Command<C> _command;

        public ChildModelCommand(Command<C> command)
        {
            _command = command;
        }

        protected internal override void Execute(M model)
        {
            _command.Execute(model.ChildFor<C>());
        }
    }
}
