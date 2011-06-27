using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Core.Queries
{
    public static class ClientQueries
    {
        public static Func<TModel, int> NumOfClients() 
        {
            return m => m.Clients.Count;
        }

        public static Func<TModel, Client> ClientByName(string clientName)
        {
            return m => m.Clients.FirstOrDefault(x => x.Name == clientName);
        }
    }
}
