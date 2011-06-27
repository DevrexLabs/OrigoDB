using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Examples.Ladders;

namespace TimeTracker.Core
{

    [Serializable]
	public class User
	{
        public string Email { get; set; }
        public String Name { get; set; }
		private String  PasswordHash { get; set; }

        public void SetPassword(string password)
        {
            PasswordHash = CryptoHelper.CreateHashWithRandomSalt(password);
        }

        public bool PasswordEquals(string password)
        {
            return CryptoHelper.VerifyHash(password, PasswordHash);
        }

		public List<Assignment> Assignments { get; set; }
	}
}
