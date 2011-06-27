using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace TimeTracker.Core.Commands
{
    [Serializable]
    public class AddClientCommand<M, T> : CommandWithResult<TModel, bool>
    {
        public string Name { get; set; }
        public List<Project> Projects { get; set; }

        protected override bool Execute(TModel model) 
        {
            Client client = new Client();
            client.Name = Name;
            client.Projects = Projects;
            model.Clients.Add(client);
            return true;
        }
    }

    [Serializable]
    public class UpdateClientCommand<M, T> : CommandWithResult<TModel, bool>
    {
        public string OldName { get; set; }
        public string NewName { get; set; }

        protected override bool Execute(TModel model)
        {
            Client client = model.Clients.FirstOrDefault(x => x.Name == OldName);
            if (client != null)
            {
                client.Name = NewName;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    [Serializable]
    public class DeleteClientCommand<M, T> : CommandWithResult<TModel, bool>
    {
        public string Name { get; set; }

        protected override bool Execute(TModel model)
        {
            Client client = model.Clients.FirstOrDefault(x => x.Name == Name);
            if (client != null)
            {
                model.Clients.Remove(client);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}