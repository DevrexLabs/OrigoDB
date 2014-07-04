using System;
using System.Linq;
using OrigoDB.Core.Security;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class AuthenticationModelTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddingNullRoleThrows()
        {
            new AuthenticationModel().AddRole(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddingEmptyRoleThrows()
        {
            new AuthenticationModel().AddRole(String.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddingWhitespaceRoleThrows()
        {
            new AuthenticationModel().AddRole(" ");
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddingDuplicateRoleThrows()
        {
            string role = Guid.NewGuid().ToString();
            var target = new AuthenticationModel();
            target.AddRole(role);
            target.AddRole(role);
        }


        [Test]
        public void CanAddRole()
        {
            string role = Guid.NewGuid().ToString();
            var target = new AuthenticationModel();
            target.AddRole(role);

            Assert.IsTrue(target.RoleExists(role));
            Assert.AreEqual(1, target.Roles.Count());
            Assert.AreEqual(role, target.Roles.Single().Name);
        }

        [Test]
        public void CanAddUserWithoutRoles()
        {
            var user = new User("robert");
            var target = new AuthenticationModel();
            target.AddUser(user);
        }

        [Test]
        public void UsersRolesAreAddedIfMissing()
        {
            var user = new User("robert");
            user.Roles.Add("admin");
            user.Roles.Add("superuser");
            var target = new AuthenticationModel();
            target.AddUser(user);
            Assert.AreEqual(2, target.Roles.Count());
            Assert.IsTrue(target.Roles.Count(r => r.Name == "admin") == 1);
            Assert.IsTrue(target.Roles.Count(r => r.Name == "superuser") == 1);
            foreach (var role in target.Roles)
            {
                Assert.IsTrue(role.Users.Single() == "robert");
            }
        }
    }
}
