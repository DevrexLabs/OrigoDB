using NUnit.Framework;
using OrigoDB.Core.Proxy;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class ProxyOverloadingTests
    {
        [Test]
        public void CanCreateMethodMap()
        {
            ProxyMethodMap.Create(typeof(ModelWithOverloads));
        }
    }
}