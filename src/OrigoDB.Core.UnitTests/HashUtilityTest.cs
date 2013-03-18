using OrigoDB.Core.Utilities;
using NUnit.Framework;
using System;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class HashUtilityTest
    {

        [Test,ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RejectsZeroSaltLength()
        {
            HashUtility.CreateHashWithRandomSalt("abc", 0);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RejectsNegativeSaltLength()
        {
            HashUtility.CreateHashWithRandomSalt("abc", -7);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void RejectsNullPlainText()
        {
            HashUtility.CreateHashWithRandomSalt(null);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void RejectsZeroLengthPlainText()
        {
            HashUtility.CreateHashWithRandomSalt(String.Empty);
        }

        [Test]
        public void HashWithDefaultSaltLengthIsVerified()
        {
            string plainText = "abc";
            string hash = HashUtility.CreateHashWithRandomSalt(plainText);
            Assert.IsTrue(HashUtility.Verify(plainText, hash));
        }
        
        [Test]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(10)]
        [TestCase(43)]
        [TestCase(100)]
        public void HashWithSpecificSaltLengthIsVerified(int saltLength)
        {
            string plainText = "abc" + saltLength.ToString();
            string hash = HashUtility.CreateHashWithRandomSalt(plainText);
            Assert.IsTrue(HashUtility.Verify(plainText, hash));
        }

    }
}
