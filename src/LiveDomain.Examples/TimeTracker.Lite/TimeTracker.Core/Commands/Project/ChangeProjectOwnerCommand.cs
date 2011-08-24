using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace TimeTracker.Core.Commands.Project
{
    [Serializable]
    public class ChangeProjectOwnerCommand<M> : Command<TModel>
    {
        public int ProjectId { get; set; }
        public String OwnerEmail { get; set; }

        protected override void Execute(TModel model)
        {
            model.ChangeOwnerForProject(ProjectId, OwnerEmail);
        }
    }
}
