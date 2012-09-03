using System.Security.Principal;

namespace LiveDomain.Core.Security
{
    public class NullAuthorizer<T> : IAuthorizer<T>
    {
        public bool Allows(T securable, IPrincipal principal)
        {
            return true;
        }
    }
}
