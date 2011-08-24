using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace TimeTracker.Core.Commands.Role
{
    [Serializable]
    public class AddRoleCommand<M> : Command<TModel>
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public decimal DefaultHourlyRate { get; set; }

        protected override void Execute(TModel model)
        {
            model.AddRole(Name, Description, DefaultHourlyRate);
        }
    }
}