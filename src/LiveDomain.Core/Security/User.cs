using System;
using System.Collections.Generic;
using LiveDomain.Core.Utilities;

namespace LiveDomain.Core.Security
{

    [Serializable]
    public class User
    {
        public readonly String Name;
        protected String PasswordHash { get; set; }
        public ISet<String> Roles { get; protected set; }

        public User(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Cant rename user: Invalid name");
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