using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrigoDB.Models.Redis;

namespace Models.Redis.Tests
{
    [TestFixture]
    public class SortedSetTests : RedisTestBase
    {
        [Test]
        public void ZAdd_using_string_params()
        {
            _target.ZAdd("key", "47", "dad", "12", "boy", "10", "girl");
            Assert.AreEqual(1, _target.KeyCount());
        }

        [Test]
        public void ZCard()
        {
            _target.ZAdd("key", "47", "dad", "12", "boy", "10", "girl");
            var numMembers = _target.ZCard("key");
            Assert.AreEqual(3, numMembers);
        }

        [Test]
        public void ZCount()
        {
            _target.ZAdd("key", "47", "dad", "12", "boy", "10", "girl");
            var zcount = _target.ZCount("key", 10, 20);
            Assert.AreEqual(2, zcount);
        }

        [Test]
        public void ZIncrementBy_existing()
        {
            _target.ZAdd("key", "47", "dad", "12", "boy", "10", "girl");
            var newScore = _target.ZIncrementBy("key", 4.0, "boy");
            Assert.AreEqual(16, newScore, 0.00000001);
        }

        [Test]
        public void ZIncrementBy_non_existing_creates_member()
        {
            _target.ZAdd("key", "47", "dad", "12", "boy", "10", "girl");
            var newScore = _target.ZIncrementBy("key", 4.0, "dog");
            Assert.AreEqual(4, newScore, 0.00000001);
            Assert.AreEqual(4, _target.ZCard("key"));
            var range = _target.ZRangeWithScores("key");
            Assert.That(range.Select(e=> e.Member), Contains.Item("dog"));
            Assert.AreEqual(range.Single(p => p.Member == "dog").Score, 4.0, 0.000001);
        }

        [Test]
        public void ZReverseRankTest()
        {
            const string k = "key";
            _target.ZAdd(k, "a", 2);
            _target.ZAdd(k, "b", 3);
            _target.ZAdd(k, "c", 5);
            _target.ZAdd(k, "d", 1);
            Assert.IsFalse(_target.ZReverseRank(k, "fish").HasValue);
            Assert.AreEqual(0, _target.ZReverseRank(k, "c"));
            Assert.AreEqual(3, _target.ZReverseRank(k, "d"));
            Assert.AreEqual(1, _target.ZReverseRank(k, "b"));
        }


        private static object[] zsetstoredata =
        {
            new object[]
            {
                new Dictionary<string,double>
                {
                    {"a", 1},
                    {"b", 2},
                    {"c", 3}
                },
                new Dictionary<string,double>
                {
                    {"a", 2},
                    {"b", 1}
                },
                2,
                new SortedSet<ZSetEntry>
                {
                    new ZSetEntry("a",3),
                    new ZSetEntry("b", 3),
                },
                RedisModel.AggregateType.Sum
            },
            new object[]
            {
                new Dictionary<string,double>
                {
                    {"a", 1},
                    {"b", 2},
                    {"c", 3}
                },
                new Dictionary<string,double>
                {
                    {"a", 2},
                    {"b", 1}
                },
                2,
                new SortedSet<ZSetEntry>
                {
                    new ZSetEntry("a",2),
                    new ZSetEntry("b",2)
                },
                RedisModel.AggregateType.Max
            },
            new object[]
            {
                new Dictionary<string,double>
                {
                    {"a", 1},
                    {"b", 2},
                    {"c", 3}
                },
                new Dictionary<string,double>
                {
                    {"a", 2},
                    {"b", 1}
                },
                2,
                new SortedSet<ZSetEntry>
                {
                    new ZSetEntry( "a",1),
                    new ZSetEntry("b",1)
                },
                RedisModel.AggregateType.Min
            },

        };

        [Test, TestCaseSource("zsetstoredata")]
        public void ZInterStoreTests(Dictionary<string,double> set1, Dictionary<string,double> set2, int expectedCount, SortedSet<ZSetEntry> expectedSet, RedisModel.AggregateType aggregateType)
        {
            _target.ZAdd("s1", set1);
            _target.ZAdd("s2", set2);
            var elementCount = _target.ZInterStore("s3", new[] { "s1", "s2" }, null, aggregateType);
            Assert.AreEqual(expectedCount, elementCount);

            CollectionAssert.AreEqual(expectedSet, _target.ZRangeWithScores("s3"));

            Console.WriteLine("s3 members:");
            foreach (var pair in _target.ZRangeWithScores("s3")) Console.WriteLine(pair);

            Console.WriteLine("s1 members:");
            foreach (var pair in _target.ZRangeWithScores("s1")) Console.WriteLine(pair);

            Console.WriteLine("s2 members:");
            foreach (var pair in _target.ZRangeWithScores("s2")) Console.WriteLine(pair);
        }

        [Test]
        public void ZRange_returns_elements_in_correct_order()
        {
            _target.ZAdd("s1", "4", "a", "2", "b", "3", "c", "3", "d");
            var expected = new[] { "b", "c", "d", "a" };
            var elements = _target.ZRange("s1");
            CollectionAssert.AreEqual(expected, elements);
            foreach (string member in elements) Console.WriteLine(member);
        }

        [Test]
        public void ZRangeByScore()
        {
            _target.ZAdd("s1", "4", "a", "2", "b", "3", "c", "3", "d", "7", "e");
            var actual = _target.ZRangeByScore("s1", min: 2, max: 5);
            var expected = new[] { "b", "c", "d", "a" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ZReverseRangeByScore()
        {
            _target.ZAdd("s1", "4", "a", "2", "b", "3", "c", "3", "d", "7", "e");
            var actual = _target.ZReverseRangeByScore("s1", min: 2, max: 5);
            var expected = new[] { "a", "d", "c", "b" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ZRank_returns_correct_rank()
        {
            _target.ZAdd("s1", "4", "a", "2", "b", "3", "c", "3", "d", "7", "e");
            var actual = _target.ZRank("s1", "c");
            Assert.IsTrue(actual.HasValue);
            Assert.AreEqual(1, actual.Value);
            foreach (var keyValuePair in _target.ZRangeWithScores("s1"))
            {
                Console.WriteLine(keyValuePair);
            }
        }

        [Test]
        public void ZRank_returns_null_for_missing_member()
        {
            _target.ZAdd("s1", "4", "a", "2", "b", "3", "c", "3", "d", "7", "e");
            var actual = _target.ZRank("s1", "g");
            Assert.IsFalse(actual.HasValue);
        }

        [Test]
        public void ZRemoveMembers()
        {
            _target.ZAdd("s1", "4", "a", "2", "b", "3", "c", "3", "d", "7", "e");
            var membersRemoved = _target.ZRemove("s1", "c", "d");
            Assert.AreEqual(2, membersRemoved);
            Assert.AreEqual(3, _target.ZCard("s1"));
        }

        [Test]
        public void ZRemoveRangeByRank()
        {
            _target.ZAdd("s1", "4", "a", "2", "b", "3", "c", "3", "d", "7", "e");
            int numRemoved = _target.ZRemoveRangeByRank("s1", 2, 4);
            Assert.AreEqual(3, numRemoved);
            string[] expected = new[] {"b", "c"};
            CollectionAssert.AreEqual(expected, _target.ZRange("s1"));
        }

        [Test]
        public void ZRemoveRangeByScore()
        {
            _target.ZAdd("s1", "4", "a", "2", "b", "3", "c", "3", "d", "7", "e");
            int numRemoved = _target.ZRemoveRangeByScore("s1", 3, 5);
            Assert.AreEqual(3, numRemoved);
            string[] expected = new[] { "b", "e" };
            CollectionAssert.AreEqual(expected, _target.ZRange("s1"));
        }


        [Test]
        public void ZScore_for_existing_member_is_retrieved()
        {
            _target.ZAdd("s1", "4", "a", "2", "b", "3", "c", "3", "d", "7", "e");
            var actual = _target.ZScore("s1", "d");
            Assert.AreEqual(3.0, actual, 0.000001);
        }

        [Test]
        public void ZScore_for_nonexisting_member_is_null()
        {
            _target.ZAdd("s1", "4", "a", "2", "b", "3", "c", "3", "d", "7", "e");
            var actual = _target.ZScore("s1", "hh");
            Assert.IsFalse(actual.HasValue);
        }


        [Test, TestCaseSource("zsetstoredata")]
        public void ZUnionStoreTests(Dictionary<string, double> set1, Dictionary<string,double> set2, int expectedCount, SortedSet<ZSetEntry> expectedSet, RedisModel.AggregateType aggregateType)
        {
            _target.ZAdd("s1", set1);
            _target.ZAdd("s2", set2);
            var elementCount = _target.ZUnionStore("s3", new[] { "s1", "s2" }, null, aggregateType);
            Assert.AreEqual(set1.Select(s => s.Key).Union(set2.Select(s => s.Key)).Count(), elementCount);

            Console.WriteLine("s3 members:");
            foreach (var pair in _target.ZRangeWithScores("s3")) Console.WriteLine(pair);

            Console.WriteLine("s1 members:");
            foreach (var pair in _target.ZRangeWithScores("s1")) Console.WriteLine(pair);

            Console.WriteLine("s2 members:");
            foreach (var pair in _target.ZRangeWithScores("s2")) Console.WriteLine(pair);
        }

        [Test]
        public void ZReverseRank()
        {
            foreach (var c in Enumerable.Range(1, 100))
            {
                _target.ZAdd("s", c.ToString(), c);
            }
            var range = _target.ZReverseRange("s", 2, 5);
            Assert.AreEqual(4, range.Length);
            CollectionAssert.AreEqual(range, new[]{"98","97", "96","95"});
        }

        [Test]
        public void ZScore_existing_is_returned()
        {
            _target.ZAdd("k", "a", 42);
            Assert.IsFalse(_target.ZScore("k","b").HasValue);
            Assert.AreEqual(42, _target.ZScore("k", "a"), 0.000001);
        }
    }
}