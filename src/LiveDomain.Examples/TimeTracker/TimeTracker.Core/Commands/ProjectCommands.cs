using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;
using TimeTracker.Core;

namespace TimeTracker.Core.Commands
{
    public class ProjectCommand
    {
        public static void GetProjectData(TModel model, int clientId, string ownerEmail, out Client client, out User owner)
        {
            client = model.Clients.SingleOrDefault(c => c.Id == clientId);
            if (client == null)
            {
                throw new Exception(String.Format("Client with Id {0} does not exist in data model", clientId));
            }

            //Has the OwnerEmail property been set to anything? If not, we don't validate because it's not a required property
            if (String.IsNullOrEmpty(ownerEmail) == false)
            {
                owner = model.Users.SingleOrDefault(u => u.Email == ownerEmail);
                if (owner == null)
                {
                    throw new Exception(String.Format("User with Email {0} does not exist in data model", ownerEmail));
                }
            }
            else
            {
                owner = null;
            }
        }
    }

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
            ProjectCommand.GetProjectData(model, ClientId, OwnerEmail, out _client, out _owner);
        }

        protected override void Execute(TModel model)
        {
            model.AddProject(Name, Description, _client, _owner);
        }
    }

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