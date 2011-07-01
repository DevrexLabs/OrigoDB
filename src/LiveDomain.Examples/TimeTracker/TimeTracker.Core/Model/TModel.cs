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
        public List<Role> Roles { get; set; }

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
            Roles = new List<Role>();
        }

        public void AddAssigmnent(User user, int projectId, int roleId, decimal hourlyRate)
        {
            Role role = GetRoleById(roleId);
            Assignment assignment = new Assignment(user, role);

            if (hourlyRate > 0)
            {
                assignment.HourlyRate = hourlyRate;
            }
            else
            {
                assignment.HourlyRate = assignment.Role.DefaultHourlyRate;
            }

            Project project = GetProjectById(projectId);
            project.Members.Add(assignment);
            user.Assignments.Add(assignment);
        }

        public void AddRole(String name, String description, decimal defaultHourlyRate)
        {
            int newId = this.Roles.GetNextId(r => r.Id);
            Role role = new Role(newId, name, description, defaultHourlyRate);
            this.Roles.Add(role);
        }

        public void EditRole(int id, string name, string description, decimal defaultHourlyRate)
        {
            Role role = this.Roles.SingleOrDefault(r => r.Id == id);
            if (role == null)
            {
                throw new ArgumentException(String.Format("Role with id {0} does not exist", id));
            }

            role.Name = name;
            role.Description = description;
            role.DefaultHourlyRate = defaultHourlyRate;
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

        private Role GetRoleById(int roleId)
        {
            Role role = this.Roles.SingleOrDefault(p => p.Id == roleId);
            if (role == null)
            {
                throw new ArgumentException("Role with the given id does not exist");
            }
            return role;
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

        #endregion
    }
}