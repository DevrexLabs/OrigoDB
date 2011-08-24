using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace TimeTracker.Core.Commands.Project
{
    [Serializable]
    public class AddProjectCommand<M> : Command<TModel>
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string OwnerEmail { get; set; }
        public int ClientId { get; set; }

        [NonSerialized]
        private Client _client;
        [NonSerialized]
        private User _owner;

        protected override void Prepare(TModel model)
        {
            _client = model.Clients.SingleOrDefault(c => c.Id == ClientId);
            if (_client == null)
            {
                throw new Exception(String.Format("Client with Id {0} does not exist in data model", ClientId));
            }

            //Has the OwnerEmail property been set to anything? If not, we don't validate because it's not a required property
            if (String.IsNullOrEmpty(OwnerEmail) == false)
            {
                _owner = model.Users.SingleOrDefault(u => u.Email == OwnerEmail);
                if (_owner == null)
                {
                    throw new Exception(String.Format("User with Email {0} does not exist in data model", OwnerEmail));
                }
            }
            else
            {
                _owner = null;
            }
        }

        protected override void Execute(TModel model)
        {
            model.AddProject(Name, Description, _client, _owner);
        }
    }
}
