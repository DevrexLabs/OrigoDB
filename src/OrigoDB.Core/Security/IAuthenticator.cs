using System.Security.Principal;

namespace OrigoDB.Core.Security
{
    public interface IAuthenticator
    {
        
        IPrincipal Authenticate(string user, string password);
        bool TryAuthenticate(string user, string password, out IPrincipal principal);
    }
}
