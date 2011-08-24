using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Core
{
    [Serializable]
    public class TModel : LiveDomain.Core.Model
    {
        public List<Client> Clients { get; set; }
        public List<Project> Projects { get; set; }
        public List<User> Users { get; set; }

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

		public void RemoveUser(User user)
		{
			if (!Users.Contains(user))
			{
				throw new Exception("User doesnt exist");
			}

			this.Users.Remove(user);
		}

        #region Project

        public void AddProject(string name, string description, Client client, User owner)
        {
            int newId = this.Projects.GetNextId(p => p.Id);
            Project newProject = new Project(newId, name, description, client);

            this.Projects.Add(newProject);
            client.Projects.Add(newProject);
        }

        public void EditProjectDetails(int id, string name, string description)
        {
            Project project = GetProjectById(id);

            project.Name = name;
            project.Description = description;
        }

        internal void ChangeClientForProject(int projectId, int clientId)
        {
            Project project = GetProjectById(projectId);

            Client oldClient = project.Client;
            oldClient.Projects.Remove(project);

            Client newClient = this.Clients.SingleOrDefault(p => p.Id == clientId);
            newClient.Projects.Add(project);

            project.Client = GetClientById(clientId);
        }

        internal void ChangeOwnerForProject(int ProjectId, string ownerEmail)
        {
            Project project = GetProjectById(ProjectId);
            project.Owner = GetUserById(ownerEmail);
        }

        #endregion

        #region Helpers

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

        #endregion
    }
}