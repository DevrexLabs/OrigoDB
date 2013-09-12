using System;
using System.Collections.Generic;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Security
{
    public class TypeBasedRule : Rule<Type>
    {
        public TypeBasedRule(Permission permission, Type securable, IEnumerable<String> roles)
            : base(permission, securable, roles)
        {
        }

        protected override bool Matches(Type securable)
        {
            //return securable.InheritsOrImplements(this.Securable);
            throw new NotImplementedException();
        }

    }
}