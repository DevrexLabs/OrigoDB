using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Principal;

namespace LiveDomain.Core.Security
{
    [Serializable]
    public class Authenticator :  IAuthenticator
    {

        public const string AuthenticationType = "LiveDBAuthentication";

        readonly Dictionary<String,Role> _roles;
        readonly Dictionary<String, User> _users;


        public Authenticator()
        {
            _roles = new Dictionary<string, Role>(StringComparer.InvariantCultureIgnoreCase);
            _users = new Dictionary<string, User>(StringComparer.InvariantCultureIgnoreCase);
        }
    
        public void AddUserToRole(string user, string role)
        {
            if(!_users.ContainsKey(user) || !_roles.ContainsKey(role)) throw new ArgumentException();
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

        /// <summary>
        /// principal is set to null if authentication fails
        /// </summary>
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
    }
}
