using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace TimeTracker.Core.Commands.Role
{
    [Serializable]
    public class EditRoleCommand<M> : Command<TModel>
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public decimal DefaultHourlyRate { get; set; }

        protected override void Execute(TModel model)
        {
            model.EditRole(Id, Name, Description, DefaultHourlyRate);
        }
    }
}