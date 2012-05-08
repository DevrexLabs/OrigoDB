using System.Security.Principal;
using System;

namespace LiveDomain.Core.Security
{
    public interface IAuthorizer<T>
    {
        bool Allows(T securable, IPrincipal principal);
    }
}
