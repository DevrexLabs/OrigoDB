using System;
using NUnit.Framework;
using OrigoDB.Core.Types;

namespace OrigoDB.Test.NUnit.Models
{
    [TestFixture]
    class RangeTests
    {
        private static object[] overlapsData =
        {
            new Object[]
            {
                new Range<int>(10, 20),
                new Range<int>(21, 30),
                false
            },
            new object[]
            {
                new Range<int>(10, 20),
                new Range<int>(19, 25),
                true
            },
            new object[]
            {
                new Range<int>(20, 30),
                new Range<int>(10, 25),
                true
            },
            new object[]
            {
                new Range<int>(10, 20),
                new Range<int>(20, 25),
                true
            },
             new object[]
            {
                new Range<int>(20, 25),
                new Range<int>(19, 20),
                true
            }
        };
        
        [Test, TestCaseSource("overlapsData")]
        public void Overlaps(Range<int> r1, Range<int>  r2, bool expected )
        {
            bool actual = r1.Overlaps(r2);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Union()
        {
            var r1 = new Range<int>(10, 20);
            var r2 = new Range<int>(15, 25);
            var union = r1.Union(r2);
            Assert.AreEqual(10, union.Start);
            Assert.AreEqual(25, union.End);
        }

        [Test]
        public void Intersection()
        {
            var r1 = new Range<int>(10, 20);
            var r2 = new Range<int>(15, 25);
            var intersection = r1.Intersect(r2);
            Assert.AreEqual(15, intersection.Start);
            Assert.AreEqual(20, intersection.End);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Constructor()
        {
            new Range<int>(10, 9);
        }

        [Test]
        public void SingleValueRange()
        {
            var target = new Range<int>(1, 1);
            Assert.AreEqual(1, target.Start);
            Assert.AreEqual(1, target.End);
        }

        [Test]
        public void ContainsValue()
        {
            var target = new Range<int>(10, 20);
            Assert.IsTrue(target.Contains(10));
            Assert.IsTrue(target.Contains(20));
            Assert.IsTrue(target.Contains(14));
        }

        [Test]
        public void PrecedesValue()
        {
            var target = new Range<int>(10, 20);
            Assert.IsTrue(target.Precedes(21));
            Assert.IsFalse(target.Precedes(20));
            Assert.IsFalse(target.Precedes(14));
            Assert.IsFalse(target.Precedes(9));
        }

        [Test]
        public void SucceedsValue()
        {
            var target = new Range<int>(10, 20);
            Assert.IsTrue(target.Succeeds(9));
            Assert.IsFalse(target.Succeeds(10));
            Assert.IsFalse(target.Succeeds(15));
            Assert.IsFalse(target.Succeeds(21));
        }
    }
}