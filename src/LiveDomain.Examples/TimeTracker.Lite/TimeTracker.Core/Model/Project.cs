using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core
{
    [Serializable]
	public class Project
	{
        [Range(0, Int32.MaxValue)]
        public int Id { get; private set; }
        [Required]
        public string Name { get; set; }        
        public string Description { get; set; }

        public User Owner { get; set; }
        public List<User> Members { get; protected set; }
        public Client Client { get; set; }

        public Project(int id, Client client) 
            : this(id, String.Empty, String.Empty, client) { }

		public Project(int id, string name, string description, Client client)
		{
            this.Id = id;
			this.Name = name;
            this.Description = description;
            this.Client = client;
			Members = new List<User>();
		}
	}
}
