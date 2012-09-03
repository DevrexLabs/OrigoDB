using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDomain.Core.Security
{
    public class TypeBasedPermissionSet : PermissionSet<Type>
    {

        public void Allow<T1>(IEnumerable<String> roles)
        {
            base.Allow(typeof(T1), roles);
        }

        public void Allow<T1, T2>(IEnumerable<String> roles)
        {
            var roleArray = roles.ToArray();
            base.Allow(typeof(T1), roleArray);
            base.Allow(typeof(T2), roleArray);
        }

        public TypeBasedPermissionSet(Permission defaultPermission = Permission.Denied) 
            : base(defaultPermission)
        {
            
        }

        //Abstract factory method
        protected override Rule<Type> CreateRule(Permission permission, Type securable, IEnumerable<string> roles)
        {
            return new TypeBasedRule(permission, securable, roles);
        }
    }
}