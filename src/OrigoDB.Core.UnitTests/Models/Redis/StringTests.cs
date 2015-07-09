using System;
using NUnit.Framework;

namespace Models.Redis.Tests
{
    [TestFixture]
    public class StringTests : RedisTestBase
    {

        private readonly object[] rangeData =
        {
            new object[]{"abcdefg", 0, 4, "abcde"},
            new object[]{"abcdefg", 4,6, "efg"},
            new Object[]{"This is a string", 0, 3, "This"},
            new Object[]{"This is a string", -3, -1, "ing"},
            new Object[]{"This is a string", 0, -1, "This is a string"},
            new Object[]{"This is a string", 10, 100, "string"},
            new Object[]{"This is a string", 100, 3, ""}




        };

        [Test, TestCaseSource("rangeData")]
        public void GetRangeTest(string value, int start, int end, string expected)
        {
            const string key = "key";
            _target.Set(key,value);
            var actual = _target.GetRange(key, start, end);
            Assert.AreEqual(expected, actual);
        }
    }
}