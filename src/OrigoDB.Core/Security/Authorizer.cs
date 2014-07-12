using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace OrigoDB.Core.Security
{
    public class Authorizer : IAuthorizer
    {
        readonly Dictionary<Type, Func<object, IPrincipal, bool>> _handlers;

        public Authorizer(Permission defaultPermission = Permission.Denied)
        {
            _handlers = new Dictionary<Type, Func<object, IPrincipal, bool>>();
            SetHandler<object>((s, p) => defaultPermission == Permission.Allowed);
        }

        public void SetHandler<T>(Func<T, IPrincipal, bool> handler)
        {
            _handlers[typeof(T)] = (s, p) => handler.Invoke((T)s, p);
        }


        public bool Allows(object securable, IPrincipal principal)
        {
            var t = GetTypeKey(securable.GetType());
            return t != null && _handlers[t].Invoke(securable, principal);
        }

        public Type GetTypeKey(Type type)
        {
            while (true)
            {
                if (_handlers.ContainsKey(type)) return type;
                if (type == typeof(object)) return null;
                type = type.BaseType;
            }
        }
    }
}