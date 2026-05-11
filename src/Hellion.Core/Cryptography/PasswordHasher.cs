using System;

namespace Hellion.Core.Cryptography
{
    /// <summary>
    /// BCrypt-based password hashing. Hashes produced here are stored as-is in
    /// the <c>users.password</c> column; verification accepts both new BCrypt
    /// hashes and pre-migration legacy strings (so existing databases keep
    /// authenticating while their rows are progressively rehashed).
    /// </summary>
    public static class PasswordHasher
    {
        private const int DefaultWorkFactor = 12;

        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, DefaultWorkFactor);
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            if (IsBCryptHash(storedHash))
            {
                try { return BCrypt.Net.BCrypt.Verify(password, storedHash); }
                catch { return false; }
            }

            return string.Equals(password, storedHash, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsBCryptHash(string hash) =>
            hash.Length >= 4 && hash[0] == '$' && hash[1] == '2'
            && (hash[2] == 'a' || hash[2] == 'b' || hash[2] == 'x' || hash[2] == 'y')
            && hash[3] == '$';
    }
}
