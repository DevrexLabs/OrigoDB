using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace OrigoDB.Core.Security
{
    
    /// <summary>
    /// A custom IPrincipal implementation using a hashset for fast role membership lookup
    /// Also, users name is added to the set of roles.
    /// </summary>
    [Serializable]
    public class OrigoDBPrincipal : IPrincipal
    {
	    public const string AuthenticationTypeName = "OrigoDBAuthentication";
	    private readonly IIdentity _identity;
        private readonly ISet<string> _roles; 

        public OrigoDBPrincipal(IIdentity identity, IEnumerable<string> roles)
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
