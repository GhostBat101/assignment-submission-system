/*
 * File: PasswordHasher.cs
 * Purpose: Secure utility for hashing passwords and verifying hashes using PBKDF2 with SHA256.
 * 
 * Dependencies Used:
 * - System.Security.Cryptography: For cryptographic salt generation and RFC2898 derivation.
 * 
 * Used By:
 * - DbInitializer.cs: To seed demo user passwords securely.
 * - AuthService.cs: To verify credentials on login.
 */

using System;
using System.Security.Cryptography;

namespace AssignmentSubmission.Infrastructure.Data
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string[] parts = hashedPassword.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hash = Convert.FromBase64String(parts[1]);

            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);
            return CryptographicOperations.FixedTimeEquals(hash, inputHash);
        }
    }
}
