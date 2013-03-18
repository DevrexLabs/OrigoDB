using System.Security.Principal;

namespace OrigoDB.Core.Security
{
    public class NullAuthorizer<T> : IAuthorizer<T>
    {
        public bool Allows(T securable, IPrincipal principal)
        {
            return true;
        }
    }
}
