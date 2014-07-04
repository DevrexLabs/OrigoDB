using System.Security.Principal;

namespace OrigoDB.Core.Security
{

    public interface IAuthorizer : IAuthorizer<object>
    {
    }

    public interface IAuthorizer<T>
    {
        bool Allows(T securable, IPrincipal principal);
    }
}
