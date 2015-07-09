using NUnit.Framework;
using OrigoDB.Core;
using OrigoDB.Models.Redis;

namespace Models.Redis.Tests
{
    [TestFixture]
    public class KeyTests : RedisTestBase
    {

        [Test]
        public void No_keys_after_clear()
        {
            _target.Set("key", "value");
            _target.Set("key2", "v2");
            _target.Clear();
            Assert.AreEqual(0, _target.KeyCount());
            Assert.IsEmpty(_target.Keys());
        }

        [Test]
        public void Existing_key_exists()
        {
            _target.Set("key", "value");
            Assert.IsTrue(_target.Exists("key"));
        }

        [Test]
        public void Removed_key_does_not_exist()
        {
            _target.Set("key", "value");
            _target.Delete("key");
            Assert.IsFalse(_target.Exists("key"));
        }

        [Test]
        public void Delete_returns_number_of_keys_deleted()
        {
            _target.Set("number", "42");
            _target.Set("name", "ringnes");
            int actual = _target.Delete("number", "name");
            Assert.AreEqual(2, actual);
        }
    }
}
