using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace OrigoDB.Core.Security
{
    [Serializable]
    public class SecurityModel : Model, IAuthenticator, IAuthorizer<Type>
    {

        protected readonly IAuthenticator _authenticator;
        protected readonly IAuthorizer _authorizer;

        public SecurityModel()
            : this(new AuthenticationModel(), new Authorizer())
        {
        }

        protected SecurityModel(IAuthenticator authenticator, IAuthorizer authorizer)
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
