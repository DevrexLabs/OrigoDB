using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using NUnit.Framework;
using OrigoDB.Core;
using OrigoDB.Core.Security;
using OrigoDB.Core.Test;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class AuthorizerTests
    {
        
        [Test]
        public void DeniedIsDefault()
        {
            var authorizer = new Authorizer();
            Assert.False(authorizer.Allows(new object(), Thread.CurrentPrincipal));
        }

        [Test]
        public void Base_class_matches()
        {
            var target = new Authorizer();
            target.SetHandler<Command>((c, p) => true);
            var cmd = new AppendNumberCommand(42);
            Assert.AreEqual(typeof (Command), target.GetTypeKey(cmd.GetType()));
            Assert.IsTrue(target.Allows(cmd, Thread.CurrentPrincipal));
        }

        [Test]
        public void Super_class_matches()
        {
            var target = new Authorizer();
            target.SetHandler<object>((c, p) => true);
            var cmd = new AppendNumberCommand(42);
            Assert.AreEqual(typeof(Object), target.GetTypeKey(cmd.GetType()));
            Assert.IsTrue(target.Allows(cmd, Thread.CurrentPrincipal));
        }

        [Test]
        public void AcceptsNullPrincipal()
        {
            var target = new Authorizer();
            Assert.DoesNotThrow(() => target.Allows(new object(), null));
        }
    }
}