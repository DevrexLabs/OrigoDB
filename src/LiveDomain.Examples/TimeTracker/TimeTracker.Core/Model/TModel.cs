using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Core
{
    [Serializable]
	public class TModel : LiveDomain.Core.Model
	{
        //private HashSet<String> _roles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        //private List<User> _people = new List<User>();
        //private List<Project> _projects = new List<Project>();

        //public IEnumerable<Project> Projects { get { return _projects; } }
        //public IEnumerable<User> People { get { return _people; } }
        //public IEnumerable<String> Roles { get { return _roles; } }

        public List<Client> Clients { get; set; }
        public List<Project> Projects { get; set; }
        public List<User> Users { get; set; }
		
        //public void AddRole(string role)
        //{
        //    if (!_roles.Contains(role))
        //    {
        //        string ucFirst = role.Substring(0, 1).ToUpperInvariant() + role.Substring(1).ToLowerInvariant();
        //        _roles.Add(ucFirst);
        //    }
        //}

		public TModel()
		{
            Clients = new List<Client>();
            Projects = new List<Project>();
            Users = new List<User>();
		}

        public void AddUser(string name, string email, string password)
        {
            if (Users.Count(u => u.Email == email) > 0)
            {
                throw new Exception("Email already exists");
            }

            User user = new User();
            user.Name = name;
            user.Email = email;
            user.SetPassword(password);
            this.Users.Add(user);
        }

        public void AddProject(string name, string description, Client client, User owner)
        {
            int newId = this.Projects.GetNextId(p => p.Id);
            Project newProject = new Project(newId, name, description, client);

            this.Projects.Add(newProject);
        }

        public void EditProjectDetails(int id, string name, string description)
        {
            Project project = GetProjectById(id);

            project.Name = name;
            project.Description = description;
        }

        internal void ChangeClientForProject(int ProjectId, int ClientId)
        {
            Project project = GetProjectById(ProjectId);
            project.Client = GetClientById(ClientId);
        }

        internal void ChangeOwnerForProject(int ProjectId, string ownerEmail)
        {
            Project project = GetProjectById(ProjectId);
            project.Owner = GetUserById(ownerEmail);
        }

        private Client GetClientById(int clientId)
        {
            Client client = this.Clients.SingleOrDefault(p => p.Id == clientId);
            if (client == null)
            {
                throw new ArgumentException("Client with the given id does not exist");
            }
            return client;
        }

        private Project GetProjectById(int id)
        {
            Project project = this.Projects.SingleOrDefault(p => p.Id == id);
            if (project == null)
            {
                throw new ArgumentException("Project with the given id does not exist");
            }
            return project;
        }

        private User GetUserById(string userEmail)
        {
            User user = this.Users.SingleOrDefault(p => p.Email == userEmail);
            if (user == null)
            {
                throw new ArgumentException("User with the given id does not exist");
            }
            return user;
        }
    }
}