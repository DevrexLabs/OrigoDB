using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using LiveDomain.Core.Security;

namespace LiveDomain.Core
{
    [Serializable]
    public class Rule<T>
    {
        public readonly Permission Permission;
        protected readonly T Securable;
        protected readonly ISet<string> Roles;

        protected Rule(Permission permission, IEnumerable<string> roles ) 
            : this(permission, default(T), roles)
        {
        }

        public Rule(Permission permission, T securable, IEnumerable<string> roles)
        {
            Permission = permission;
            Securable = securable;
            Roles = new HashSet<string>(roles, StringComparer.InvariantCultureIgnoreCase);
        }

        public virtual bool Matches(T securable, IPrincipal principal)
        {
            return Matches(securable) &&
                   (Roles.Contains("*") || Roles.Any(principal.IsInRole));
        }

        protected virtual bool Matches(T securable)
        {
            return Securable.Equals(securable);
        }


    }
}