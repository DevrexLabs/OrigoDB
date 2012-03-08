using System;
using LiveDomain.Core.Security;

namespace LiveDomain.Core
{
    [Serializable]
    public class StaticRule<T> : Rule<T>
    {
        public StaticRule(Permission permission)
            :base(permission, new string[]{})
        {
            
        }
        protected override bool Matches(T securable)
        {
            return true;
        }
    
    }
}