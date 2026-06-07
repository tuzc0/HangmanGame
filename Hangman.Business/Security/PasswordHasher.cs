using Hangman.Business.Configuration;
using System;
using System.Security.Cryptography;

namespace Hangman.Business.Security
{
    public static class PasswordHasher
    {
        private const string Algorithm = "PBKDF2-SHA256";

        public static string HashPassword(string password, AuthSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is required.", nameof(password));
            }

            byte[] salt = new byte[settings.PasswordSaltSize];

            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(salt);
            }

            byte[] hash;

            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                settings.PasswordIterations,
                HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(settings.PasswordHashSize);
            }

            return string.Format(
                "{0}${1}${2}${3}",
                Algorithm,
                settings.PasswordIterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(string password, string storedPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedPasswordHash))
            {
                return false;
            }

            string[] parts = storedPasswordHash.Split('$');

            if (parts.Length != 4 || parts[0] != Algorithm)
            {
                return false;
            }

            int iterations;

            if (!int.TryParse(parts[1], out iterations))
            {
                return false;
            }

            byte[] salt;
            byte[] storedHash;

            try
            {
                salt = Convert.FromBase64String(parts[2]);
                storedHash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] computedHash;

            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256))
            {
                computedHash = pbkdf2.GetBytes(storedHash.Length);
            }

            return SlowEquals(storedHash, computedHash);
        }

        private static bool SlowEquals(byte[] first, byte[] second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            int difference = first.Length ^ second.Length;
            int length = Math.Min(first.Length, second.Length);

            for (int index = 0; index < length; index++)
            {
                difference |= first[index] ^ second[index];
            }

            return difference == 0;
        }
    }
}
