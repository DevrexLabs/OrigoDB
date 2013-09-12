using System;
using System.Collections.Generic;

namespace OrigoDB.Core.Security
{
    [Serializable]
    public class Role
    {
        public String Name { get; private set; }
        public ISet<String> Users { get; private set; }


        public void Rename(string name)
        {
            if(String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Invalid role name");
            }
            Name = name;
        }

        public Role(string name)
        {
            Rename(name);
            Users = new HashSet<String>(StringComparer.InvariantCultureIgnoreCase);
        }
    }
}