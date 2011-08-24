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

        protected override bool Execute(TModel model) 
        {
            int newId = 0;
            Client mostRecentClient = model.Clients.LastOrDefault();
            if (mostRecentClient != null)
            {
                newId = mostRecentClient.Id + 1;
            }

            Client client = new Client();
            client.Id = newId;
            client.Name = Name;
            model.Clients.Add(client);
            return true;
        }
    }

    [Serializable]
    public class UpdateClientCommand<M, T> : CommandWithResult<TModel, bool>
    {
        public int Id { get; set; }
        public string NewName { get; set; }

        protected override bool Execute(TModel model)
        {
            Client client = model.Clients.FirstOrDefault(x => x.Id == Id);
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