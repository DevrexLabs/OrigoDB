using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core.Security;
using NUnit.Framework;

namespace LiveDomain.Core.UnitTests
{
    [TestFixture]
    public class AuthenticationModelTests
    {
        [Test]
        public void CanCreateAuthenticationModel()
        {
            new AuthenticationModel();
        }

        [Test]
        public void CanAddRole()
        {
            string role = Guid.NewGuid().ToString();
            var target = new AuthenticationModel();
            target.AddRole(role);
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
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
        public void DuplicateRoleCannotBeAdded()
        {
            string role = Guid.NewGuid().ToString();
            var target = new AuthenticationModel();
            target.AddRole(role);
            target.AddRole(role);
        }


        [Test]
        public void AddedRoleIsReturned()
        {
            string role = Guid.NewGuid().ToString();
            var target = new AuthenticationModel();
            target.AddRole(role);

            Assert.IsTrue(target.RoleExists(role));
        }
    }
}
