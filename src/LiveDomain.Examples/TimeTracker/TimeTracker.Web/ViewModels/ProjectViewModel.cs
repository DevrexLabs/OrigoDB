using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeTracker.Core;

namespace TimeTracker.Web.ViewModels
{
	public class ProjectViewModel
	{
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SelectedClientId{ get; set; }
        public List<Client> Clients { get; set; }

        public ProjectViewModel()
        {
            Clients = new List<Client>();
        }
    }
}