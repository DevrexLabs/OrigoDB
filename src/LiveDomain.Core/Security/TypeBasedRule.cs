using System;
using System.Collections.Generic;
using LiveDomain.Core.Utilities;

namespace LiveDomain.Core.Security
{
    public class TypeBasedRule : Rule<Type>
    {
        public TypeBasedRule(Permission permission, Type securable, IEnumerable<String> roles)
            : base(permission, securable, roles)
        {
        }

        protected override bool Matches(Type securable)
        {
            return securable.InheritsOrImplements(this.Securable);
        }

    }
}