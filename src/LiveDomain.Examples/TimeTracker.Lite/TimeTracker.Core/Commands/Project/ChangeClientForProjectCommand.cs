using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace TimeTracker.Core.Commands.Project
{
    [Serializable]
    public class ChangeClientForProjectCommand<M> : Command<TModel>
    {
        public int ProjectId { get; set; }
        public int ClientId { get; set; }

        protected override void Execute(TModel model)
        {
            model.ChangeClientForProject(ProjectId, ClientId);
        }
    }
}
