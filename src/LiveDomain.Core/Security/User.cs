using System;
using System.Collections.Generic;
using LiveDomain.Core.Utilities;

namespace LiveDomain.Core.Security
{
    [Serializable]
    public class User
    {
        public String Name { get; protected set; }
        protected String PasswordHash { get; set; }
        public ISet<String> Roles { get; protected set; }


        public void Rename(string newName)
        {
            if (String.IsNullOrWhiteSpace(newName)) throw new ArgumentException("Cant rename user: Invalid name");
            Name = newName;
        }

        public User(string name)
        {
            Roles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            PasswordHash = String.Empty;
        }
        
        public bool HasPassword(string password)
        {
            return PasswordHasher.Verify(password, PasswordHash);
        }

        public void SetPassword(string password)
        {
            PasswordHash = PasswordHasher.CreateHashWithRandomSalt(password);
        }
    }
}