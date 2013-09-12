using System.Security.Principal;
using System;

namespace OrigoDB.Core.Security
{
    public interface IAuthorizer<T>
    {
        bool Allows(T securable, IPrincipal principal);
    }
}
