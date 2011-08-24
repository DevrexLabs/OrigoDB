using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace TimeTracker.Core.Commands.Assignment
{
    [Serializable]
    public class AddAssignmentCommand<M> : Command<TModel>
    {
        public User User { get; set; }
        public int ProjectId { get; set; }
        public int RoleId { get; set; }
        public decimal HourlyRate { get; set; }

        protected override void Execute(TModel model)
        {
            model.AddAssigmnent(User, ProjectId, RoleId, HourlyRate);
        }
    }
}