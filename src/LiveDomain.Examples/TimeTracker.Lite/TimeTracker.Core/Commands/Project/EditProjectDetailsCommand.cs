using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace TimeTracker.Core.Commands.Project
{
    [Serializable]
    public class EditProjectDetailsCommand<M> : Command<TModel>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        protected override void Execute(TModel model)
        {
            model.EditProjectDetails(Id, Name, Description);
        }
    }
}
