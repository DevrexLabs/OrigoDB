using System;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OrigoDB.Core.Modeling.Redis;

namespace Models.Redis.Tests
{
    [TestFixture]
    public class BitsetTests : RedisTestBase
    {

        private readonly object[] setAndGetData =
        {
            0,
            5,
            7

        };

        [Test, TestCaseSource("setAndGetData")]
        public void SetAndGet(int offset)
        {
            const string key = "a";
            bool original = _target.SetBit(key, 0, true);
            Assert.AreEqual(false, original);
            bool current = _target.GetBit(key, 0);
            Assert.AreEqual(true, current);
        }

        [Test]
        public void BitPos()
        {
            const string key = "a";
            int[] indices = {4,7,12,15,16,20,23,200};
            foreach (var index in indices)
            {
                _target.SetBit(key, index, true);
            }


            foreach (var index in indices)
            {
                int actual = _target.BitPos(key, true, index);
                Assert.AreEqual(index, actual);
            }

            foreach (var index in indices)
            {
                int actual = _target.BitPos(key, true);
                Assert.AreEqual(index, actual);
                _target.SetBit(key, index, false);
            }
        }

        private string RandomBitString(Random r, int length)
        {
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                if (r.NextDouble() > 0.5) sb.Append("1");
                else sb.Append("0");
            }
            return sb.ToString().TrimEnd('0');
        }

        private void CreateBitString(string key, string bitString)
        {
            _target.Delete(key);
            for (int i = 0; i < bitString.Length; i++)
            {
                if (BitAt(bitString, i)) _target.SetBit(key, i, true);
            }
        }

        private static readonly object[] ops =
        {
            RedisModel.BitOperator.And, 
            RedisModel.BitOperator.Or, 
            RedisModel.BitOperator.Xor, 
            RedisModel.BitOperator.Not
        };

        [Test, TestCaseSource("ops")]
        public void BitwiseOperators(RedisModel.BitOperator op)
        {
            var r = new Random(0);
            var a = RandomBitString(r, 100);
            var b = RandomBitString(r, 104);
            CreateBitString("a", a);
            Assert.True(Equals(a, "a"));
            CreateBitString("b", b);
            Assert.True(Equals(b,"b"));
            var result = DoBitOperation(op, a, b);
            _target.BitOp(op, "c", "a", "b");

            Console.WriteLine(op);
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(result);

            Assert.True(Equals(result, "c"));

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
        private string DoBitOperation(RedisModel.BitOperator op, string a, string b)
        {
            var regex = new Regex("^[01]+$");
            if (!regex.IsMatch(a) || !regex.IsMatch(b)) throw new ArgumentException("string must contain only zeros and ones");
            StringBuilder result = new StringBuilder(Math.Max(a.Length,b.Length));
            for (int i = 0; i < Math.Max(a.Length,b.Length); i++)
            {
                switch (op)
                {
                    case RedisModel.BitOperator.And:
                        result.Append(BitAt(a, i) && BitAt(b, i) ? "1" : "0");
                        break;
                    case RedisModel.BitOperator.Or:
                        result.Append(BitAt(a, i) || BitAt(b, i) ? "1" : "0");
                        break;
                    case RedisModel.BitOperator.Xor:
                        result.Append(BitAt(a, i) ^ BitAt(b, i) ? "1" : "0");
                        break;
                    case RedisModel.BitOperator.Not:
                        result.Append(BitAt(a, i) ? "0" : "1");
                        break;
                }
            }
            return result.ToString().TrimEnd('0');
        }

        private bool BitAt(string bitString, int offset)
        {
            if (bitString.Length <= offset) return false;
            return bitString[offset] == '1';
        }

    }
}