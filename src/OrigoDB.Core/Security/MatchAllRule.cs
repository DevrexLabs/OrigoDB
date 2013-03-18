using System;
using OrigoDB.Core.Security;
using System.Security.Principal;

namespace OrigoDB.Core.Security
{

    [Serializable]
    public class MatchAllRule<T> : Rule<T>
    {
        public MatchAllRule(Permission permission)
            :base(permission, new string[]{})
        {
        }

        protected override bool Matches(T securable)
        {
            return true;
        }

        public override bool Matches(T securable, IPrincipal principal)
        {
            return true;
        }
    
    }
}