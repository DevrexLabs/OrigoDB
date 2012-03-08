using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace LiveDomain.Core.Security
{
    
    /// <summary>
    /// A custom IPrincipal implementation using a hashset for fast role membership lookup
    /// Also, users name is added to the set of roles.
    /// </summary>
    [Serializable]
    public class LiveDomainPrincipal : IPrincipal
    {
        private readonly IIdentity _identity;
        private readonly ISet<String> _roles; 

        public LiveDomainPrincipal(IIdentity identity, IEnumerable<String> roles)
        {
            _identity = identity;
            _roles = new HashSet<string>(roles, StringComparer.InvariantCultureIgnoreCase);
            _roles.Add(_identity.Name);
        }

        public IIdentity Identity
        {
            get
            {
                return _identity;
            }
        }

        public bool IsInRole(string role)
        {
            return _roles.Contains(role);
        }
    }
}
