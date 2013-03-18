using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Principal;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Security
{
    [Serializable]
    public class AuthenticationModel : IAuthenticator
    {

        public const string AuthenticationType = "LiveDomainAuthentication";

        readonly Dictionary<String, Role> _roles;
        readonly Dictionary<String, User> _users;


        public AuthenticationModel()
        {
            _roles = new Dictionary<string, Role>(StringComparer.InvariantCultureIgnoreCase);
            _users = new Dictionary<string, User>(StringComparer.InvariantCultureIgnoreCase);
        }

        public IEnumerable<Role> Roles
        {
            get
            {
                foreach (var role in _roles.Values)
                {
                    yield return role;
                }
            }
        }

        public virtual void AddUser(User user)
        {
            if (_users.ContainsKey(user.Name)) throw new ArgumentException("Username already exists");
            _users.Add(user.Name, user);
            foreach (string role in user.Roles)
            {
                if (!_roles.ContainsKey(role)) AddRole(role);
                AddUserToRole(user.Name, role);
            }
        }

        public void AddRole(string roleName)
        {
            Ensure.NotNull(roleName, "roleName");
            if(_roles.ContainsKey(roleName)) throw new ArgumentException("Role already exists");
            Role role = new Role(roleName);
            _roles.Add(roleName, role);
        }

        public void AddUserToRole(string user, string role)
        {
            if (!_users.ContainsKey(user) || !_roles.ContainsKey(role)) throw new ArgumentException();
            _roles[role].Users.Add(user);
            _users[user].Roles.Add(role);
        }

        public void RemoveUserFromRole(string user, string role)
        {
            if (!_users.ContainsKey(user) || !_roles.ContainsKey(role)) throw new ArgumentException();
            _roles[role].Users.Remove(user);
            _users[user].Roles.Remove(role);
        }

        public IPrincipal Authenticate(string userName, string password)
        {
            User user;
            if (_users.TryGetValue(userName, out user))
            {
                if (user.HasPassword(password))
                {
                    return PrincipalForUser(user);
                }
            }
            throw new AuthenticationException();
        }

        private IPrincipal PrincipalForUser(User user)
        {
            var identity = new GenericIdentity(user.Name, AuthenticationType);
            return new LiveDomainPrincipal(identity, user.Roles);
        }

        public bool TryAuthenticate(string userName, string plaintextPassword, out IPrincipal principal)
        {
            principal = null;
            try
            {

                principal = Authenticate(userName, plaintextPassword);
                return true;
            }
            catch (AuthenticationException)
            {
                return false;
            }
        }

        public bool RoleExists(string role)
        {
            return _roles.ContainsKey(role);
        }
    }
}
