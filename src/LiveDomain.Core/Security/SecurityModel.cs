using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace LiveDomain.Core.Security
{
    [Serializable]
    public class SecurityModel : Model, IAuthenticator, IAuthorizer<Type>
    {

        protected readonly IAuthenticator _authenticator;
        protected readonly IAuthorizer<Type> _authorizer;

        public SecurityModel()
            : this(new AuthenticationModel(), new TypeBasedPermissionSet())
        {
        }

        protected SecurityModel(IAuthenticator authenticator, IAuthorizer<Type> authorizer)
        {
            _authorizer = authorizer;
            _authenticator = authenticator;

        }

        public IPrincipal Authenticate(string user, string password)
        {
            return _authenticator.Authenticate(user, password);
        }

        public bool TryAuthenticate(string user, string password, out IPrincipal principal)
        {
            return _authenticator.TryAuthenticate(user, password, out principal);
        }

        public bool Allows(Type securable, IPrincipal principal)
        {
            return _authorizer.Allows(securable, principal);
        }
    }
}
