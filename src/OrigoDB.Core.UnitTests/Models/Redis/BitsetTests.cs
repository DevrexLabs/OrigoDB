using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OrigoDB.Core.Modeling.Redis;

namespace Models.Redis.Tests
{
    [TestFixture]
    public class BitsetTests : RedisTestBase
    {

        private IEnumerable<int> RandomInts(int seed, int n, int upperBound)
        {
            var r = new Random(seed);
            for (int i = 0; i < n; i++)
            {
                yield return r.Next(upperBound);
            }
        }

        [Test]
        public void SetAndGet()
        {
            const string key = "a";
            bool original = _target.SetBit(key, 0);
            Assert.AreEqual(false, original);
            bool current = _target.GetBit(key, 0);
            Assert.AreEqual(true, current);
        }


        [Test]
        public void SetBitReturnsPreviousValue()
        {
            bool actual = _target.SetBit("a", 42, true);
            Assert.AreEqual(false, actual, "should return false on initial call to set");

            actual = _target.SetBit("a", 42, false);
            Assert.AreEqual(true, actual, "expected true as set in the previous call");

            actual = _target.SetBit("a", 42, true);
            Assert.AreEqual(false, actual, "false returned when previously set ");
        }

        [Test]
        public void GetWhenNotPreviouslySet()
        {
            var r = new Random(0);
            var actual = _target.GetBit("a", r.Next());
            Assert.AreEqual(false, actual);
        }

        [Test]
        public void BitPos()
        {
            var seed = Environment.TickCount;
            var r = new Random(seed);
            for (int i = 0; i < 20; i++)
            {
                var randomBitString = RandomBitString(r, 50);
                foreach (var pos in RandomInts(seed, 20, 45))
                {
                    var expected = BitPosImpl(randomBitString, true, pos);
                    CreateBitString("a", randomBitString);
                    var actual = _target.BitPos("a", true, pos);
                    Assert.AreEqual(expected, actual, "failed for bitstring " + randomBitString + ", seed " + seed);
                }
            }

        }

        private int BitPosImpl(string bitString, bool value, int startIndex = 0)
        {
            return bitString
                .ToCharArray()
                .Skip(startIndex)
                .Select((c, i) => Tuple.Create(c == '1', i + startIndex))
                .Where(t => t.Item1 == value).DefaultIfEmpty(Tuple.Create(value, -1))
                .Select(t => t.Item2)
                .First();
        }

        private string RandomBitString(Random r, int length)
        {
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                if (r.NextDouble() > 0.5) sb.Append("1");
                else sb.Append("0");
            }
            return sb.ToString();
        }

        private void CreateBitString(string key, string bitString)
        {
            _target.Delete(key);
            for (int i = 0; i < bitString.Length; i++)
            {
                if (BitAt(bitString, i)) _target.SetBit(key, i, true);
            }

            //set last bit if 0, forcing correct length
            var last = bitString.Length - 1;
            if (!BitAt(bitString, last)) _target.SetBit(key, last, false);
        }

        private static readonly object[] Ops =
        {
            BitOperator.And, 
            BitOperator.Or, 
            BitOperator.Xor, 
            BitOperator.Not
        };

        [Test, TestCaseSource("Ops")]
        public void BitwiseOperators(BitOperator op)
        {
            var seed = new Random().Next();
            var r = new Random(seed);
            var a = RandomBitString(r, 104);
            var b = RandomBitString(r, 100);
            CreateBitString("a", a);
            Assert.True(Equals(a, "a"));
            CreateBitString("b", b);
            Assert.True(Equals(b,"b"));
            var result = DoBitOperation(op, a, b);
            if (op != BitOperator.Not) _target.BitOp(op, "c", "a", "b");
            else _target.BitOp(op, "c", "a");

            Console.WriteLine(op);
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(result);

            Assert.True(Equals(result, "c"), "failed with seed " + seed);

        }


        [Test]
        public void BitCountRange()
        {
            const string key = "a";
            String bs = "10101010101";
            CreateBitString(key, bs);

            Assert.AreEqual(6, _target.BitCount(key));
            Assert.AreEqual(5, _target.BitCount(key,1));
            Assert.AreEqual(4, _target.BitCount(key, 1, 9));

            Assert.AreEqual(6, _target.BitCount(key, 0, -1));
            Assert.AreEqual(5, _target.BitCount(key, 0, -3));

        }

        [Test]
        public void BitCount()
        {
            const string key = "a";
            int numBitsSet = 0;
            Random r = new Random(0);
            for (var i = 0; i < 1000; i++)
            {
                if (r.NextDouble() > 0.5)
                {
                    _target.SetBit(key, i, true);
                    numBitsSet++;
                }
            }
            int actual = _target.BitCount(key);
            Assert.AreEqual(numBitsSet, actual);
        }

        [Test]
        public void Length()
        {
            const string key = "a";
            const int limit = 100000;

            //seed is random but included in output if test fails
            var seed = new Random().Next();
            var r = new Random(seed);
            int max = 0;
            for (int i = 0; i < 1000; i++)
            {
                int index = r.Next(limit);
                max = Math.Max(index, max);
                _target.SetBit(key, index);
                var actual = _target.StrLength(key);
                Assert.AreEqual(max + 1, actual, "failed at iteration " + i + "using seed " + seed);
            }
        }

        /// <summary>
        /// BitArray with key key at _target is equivalent to the passed bitstring
        /// </summary>
        /// <param name="bitString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool Equals(String bitString, String key)
        {
            for (var i = 0; i < bitString.Length; i++)
            {
                if (_target.GetBit(key, i) != BitAt(bitString, i)) return false;
            }
            return true;
        }
        private string DoBitOperation(BitOperator op, string a, string b)
        {
            var regex = new Regex("^[01]+$");
            if (!regex.IsMatch(a) || !regex.IsMatch(b)) throw new ArgumentException("string must contain only zeros and ones");
            StringBuilder result = new StringBuilder(Math.Max(a.Length,b.Length));
            for (int i = 0; i < Math.Max(a.Length,b.Length); i++)
            {
                switch (op)
                {
                    case BitOperator.And:
                        result.Append(BitAt(a, i) && BitAt(b, i) ? "1" : "0");
                        break;
                    case BitOperator.Or:
                        result.Append(BitAt(a, i) || BitAt(b, i) ? "1" : "0");
                        break;
                    case BitOperator.Xor:
                        result.Append(BitAt(a, i) ^ BitAt(b, i) ? "1" : "0");
                        break;
                    case BitOperator.Not:
                        result.Append(BitAt(a, i) ? "0" : "1");
                        break;
                }
            }
            return result.ToString();
        }

        private bool BitAt(string bitString, int offset)
        {
            if (bitString.Length <= offset) return false;
            return bitString[offset] == '1';
        }

    }
}