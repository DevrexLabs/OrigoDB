using OrigoDB.Core.Security;
using NUnit.Framework;
using System.Security.Principal;

namespace OrigoDB.Core.UnitTests
{
    [TestFixture]
    public class SuccessfulUserAuthentication
    {
        private AuthenticationModel target;
        private IPrincipal principal;
        string userName = "Homer";

        [TestFixtureSetUp]
        public void Setup()
        {
            //Arrange
            string password = "donuts!";
            target = new AuthenticationModel();
            var user = new User(userName);
            user.Roles.Add("admin");
            user.SetPassword(password);
            target.AddUser(user);

            //Act
            principal = target.Authenticate(userName, password);
        }


        [Test]
        public void UserIsAuthenticated()
        {
            //Assert
            Assert.IsTrue(principal.Identity.IsAuthenticated);
        }

        [Test]
        public void IdentityHasUserName()
        {
            Assert.AreEqual(userName, principal.Identity.Name);
        }

        [Test]
        public void LiveDomainAuthenticationMethodIsReported()
        {
            Assert.IsTrue(principal.Identity.AuthenticationType == "LiveDomainAuthentication");
        }

        [Test]
        public void UserIsInInitialRole()
        {
            Assert.IsTrue(principal.IsInRole("admin"));
        }
    }
}
