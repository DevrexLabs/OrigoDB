using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FakeItEasy;
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
            Assert.AreEqual(role, target.Roles.First().Name);
        }

        [Test]
        public void CanAddUserWithoutRoles()
        {
            var userMock = new Fake<User>(builder => builder.WithArgumentsForConstructor(() => new User("robert")));
            var hashSet = new HashSet<string>();
            A.CallTo(() => userMock.FakedObject.Roles).Returns(hashSet);
            var target = new AuthenticationModel();
            target.AddUser(userMock.FakedObject);

            
        }
    }
}
