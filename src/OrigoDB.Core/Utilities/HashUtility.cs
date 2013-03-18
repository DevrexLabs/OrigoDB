using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using OrigoDB.Core.Logging;

namespace OrigoDB.Core.Utilities
{

    /// <summary>
    /// Utility class to hash strings and verify hashes using the SHA512 algorithm
    /// </summary>
    public static class HashUtility
    {

        private static ILog _log = LogProvider.Factory.GetLogForCallingType();

        public const byte DefaultSaltLength = 8;


        /// <summary>
        /// Generate a hash of the provided string using the SHA512 algorithm. 
        /// Random salt bytes are appended to the hash
        /// </summary>
        /// <param name="plainText">The string to hash</param>
        /// <param name="saltLength">the length of the random salt byte array, see <see cref="DefaultSaltLength"/></param>
        /// <returns>A hash of the provided plainText with the salt appended</returns>
        public static string CreateHashWithRandomSalt(string plainText, int saltLength = DefaultSaltLength)
        {
            Ensure.NotNullOrEmpty(plainText, "plainText");
            if (saltLength <= 0) throw new ArgumentOutOfRangeException("saltLength", "salt length must be >= 0");
            byte[] salt = RandomBytes(saltLength);
            return ComputeHash(plainText, salt);
        }

        /// <summary>
        /// Verifys that a hash was created from a given plaintext string.
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="hash"></param>
        /// <param name="saltLength"></param>
        /// <returns>true if the hash could be created from the plaintext</returns>
        public static bool Verify(string plainText, string hash, int saltLength = DefaultSaltLength)
        {
            try
            {
                byte[] hashWithSaltBytes = Convert.FromBase64String(hash);

                //extract the salt from the end of the array
                byte[] salt = new byte[saltLength];
                int hashSizeInBytes = hashWithSaltBytes.Length - saltLength;
                Array.ConstrainedCopy(hashWithSaltBytes, hashSizeInBytes, salt, 0, saltLength);

                //recompute and compare
                return hash == ComputeHash(plainText, salt);
            }
            catch (Exception ex)
            {
                _log.Error("Bad input to HashUtility.Verify");
                _log.Exception(ex);
                return false;
            }
        }

        private static byte[] RandomBytes(int size)
        {
            byte[] result = new byte[size];
            RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
            rnd.GetNonZeroBytes(result);
            return result;
        }

        private static string ComputeHash(string plainText, byte[] salt)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // put the plainText and salt into a single array
            byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + salt.Length];
            plainTextBytes.CopyTo(plainTextWithSaltBytes, 0);
            salt.CopyTo(plainTextWithSaltBytes, plainTextBytes.Length);

            //Compute
            HashAlgorithm hasher = new SHA512Managed();
            byte[] hashBytes = hasher.ComputeHash(plainTextWithSaltBytes);

            // Combine the hash and the salt to a single array
            byte[] resultBytes = new byte[hashBytes.Length + salt.Length];
            hashBytes.CopyTo(resultBytes,0);
            salt.CopyTo(resultBytes, hashBytes.Length);

            return Convert.ToBase64String(resultBytes);
        }
    }
}
