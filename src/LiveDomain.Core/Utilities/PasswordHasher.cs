using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace LiveDomain.Core.Utilities
{
    public static class PasswordHasher
    {

        public const byte DefaultSaltLength = 8;


        public static string CreateHashWithRandomSalt(string password, int saltLength = DefaultSaltLength)
        {
            byte[] salt = RandomBytes(saltLength);
            return ComputeHash(password, salt);
        }

        public static bool Verify(string password, string passwordHash, int saltLength = DefaultSaltLength)
        {
            // Convert base64-encoded hash value into a byte array.
            byte[] hashWithSaltBytes = Convert.FromBase64String(passwordHash);

            int hashSizeInBytes = hashWithSaltBytes.Length - saltLength;


            // Allocate array to hold original salt bytes retrieved from hash.
            byte[] salt = new byte[saltLength];

            // Copy salt from the end of the hash to the new array.
            for (int i = 0; i < salt.Length; i++)
                salt[i] = hashWithSaltBytes[hashSizeInBytes + i];


            // Compute a new hash string.
            string expectedHashString =
                        ComputeHash(password, salt);

            // If the computed hash matches the specified hash,
            // the plain text value must be correct.
            return (passwordHash == expectedHashString);
        }

        private static byte[] RandomBytes(int size)
        {
            byte[] result = new byte[size];
            RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
            rnd.GetNonZeroBytes(result);
            return result;
        }

        private static string ComputeHash(string password, byte[] salt)
        {
            // Convert plain text into a byte array.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(password);

            // Allocate array, which will hold plain text and salt.
            byte[] plainTextWithSaltBytes =
                    new byte[plainTextBytes.Length + salt.Length];

            // Copy plain text bytes into resulting array.
            plainTextBytes.CopyTo(plainTextWithSaltBytes, 0);

            // Append salt bytes to the resulting array.
            salt.CopyTo(plainTextWithSaltBytes, plainTextBytes.Length);

            HashAlgorithm hasher = new SHA512Managed();

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hasher.ComputeHash(plainTextWithSaltBytes);

            // Create array which will hold hash and original salt bytes.
            byte[] hashWithSaltBytes = new byte[hashBytes.Length +
                                                salt.Length];

            // Copy hash bytes into resulting array.
            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            // Append salt bytes to the result.
            for (int i = 0; i < salt.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = salt[i];

            // Convert result into a base64-encoded string.
            return Convert.ToBase64String(hashWithSaltBytes);
        }



    }
}
