using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationSystem.Utilities
{
    public static class PasswordSecurity
    {
        public static class PasswordHasher
        {
            private const int SaltSize = 16; //128bit
            private const int HashSize = 32; //256bit
            private const int MemoryCost = 19456; //19MB
            private const int TimeCost = 2; //2 iterations
            private const int Parallelism = 1; //prevent cpu overload

            private static readonly byte[] Pepper = Encoding.UTF8.GetBytes("z2U@nP9qR2tY4uV6wX2cB0cD3eF5gR8j");


            public static string HashPassword(string password)
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Password cannot be empty.");

                byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] pepperedPassword = new byte[passwordBytes.Length + Pepper.Length];
                Buffer.BlockCopy(passwordBytes,0,pepperedPassword,0,passwordBytes.Length);
                Buffer.BlockCopy(Pepper,0,pepperedPassword,passwordBytes.Length,Pepper.Length);


                var argon2 = new Argon2id(pepperedPassword)
                {
                    Salt = salt,
                    DegreeOfParallelism = Parallelism,
                    MemorySize = MemoryCost,
                    Iterations = TimeCost
                };

                byte[] hash = argon2.GetBytes(HashSize);

                byte[] result = new byte[SaltSize + HashSize];

                Buffer.BlockCopy(salt,0,result,0,SaltSize);
                Buffer.BlockCopy(hash, 0, result, SaltSize,HashSize);

                return Base64UrlEncode(result);
            }

            public static bool VerifyPassword(string password, string storedHash)
            {
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                    return false;

                storedHash = storedHash.Trim();
                byte[] storedBytes;

                try
                {
                    storedBytes = Base64UrlDecode(storedHash);
                }
                catch (Exception e)
                {
                    try
                    {
                        storedBytes = Convert.FromBase64String(storedHash);
                    }
                    catch (Exception exception)
                    {
                        return false;
                    }
                }

                if(storedBytes.Length != SaltSize + HashSize)
                    return false;

                byte[] salt = new byte[SaltSize];
                byte[] expectedHash = new byte[HashSize];
                Buffer.BlockCopy(storedBytes,0,salt,0,SaltSize);
                Buffer.BlockCopy(storedBytes,SaltSize,expectedHash,0,HashSize);

                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] pepperedPassword = new byte[passwordBytes.Length + Pepper.Length];
                Buffer.BlockCopy(passwordBytes, 0, pepperedPassword, 0, passwordBytes.Length);
                Buffer.BlockCopy(Pepper, 0, pepperedPassword, passwordBytes.Length, Pepper.Length);

                var argon2 = new Argon2id(pepperedPassword)
                {
                    Salt = salt,
                    DegreeOfParallelism = Parallelism,
                    MemorySize = MemoryCost,
                    Iterations = TimeCost
                };

                byte[] computedHash = argon2.GetBytes(HashSize);

                return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
            }
        }

        public static class PasswordStrengthChecker
        {
            private static readonly string[] CommonPasswords = {
        "123456", "123456789", "12345678", "password", "123123", "000000",
        "111111", "1234567", "qwerty", "abc123", "password1", "1234567890",
        "admin", "user", "test", "1234", "12345", "password123"
    };

            public static (int Score, string Message, string StrengthClass) Evaluate(string password, string phoneNumber = "", string fullName = "")
            {
                if (string.IsNullOrWhiteSpace(password))
                    return (0, "رمز عبور نمی‌تواند خالی باشد", "danger");

                if (password.Length < 8)
                    return (10, "رمز عبور باید حداقل ۸ کاراکتر باشد", "danger");

                int score = 0;
                var feedback = new List<string>();

                // طول
                if (password.Length >= 12) score += 30;
                else if (password.Length >= 10) score += 20;
                else if (password.Length >= 8) score += 10;

                // حرف کوچک
                if (Regex.IsMatch(password, @"[a-z]")) { score += 15; feedback.Add("حرف کوچک"); }

                // حرف بزرگ
                if (Regex.IsMatch(password, @"[A-Z]")) { score += 20; feedback.Add("حرف بزرگ"); }

                // عدد
                if (Regex.IsMatch(password, @"[0-9]")) { score += 15; feedback.Add("عدد"); }

                // کاراکتر خاص
                if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")) { score += 20; feedback.Add("کاراکتر خاص"); }

                // تکرار نشدن کاراکترها
                if (password.Distinct().Count() > password.Length / 2) score += 10;

                // بلاک کردن رمزهای معروف
                if (CommonPasswords.Contains(password.ToLower()))
                    return (0, "این رمز عبور بسیار ضعیف و رایج است!", "danger");

                // بلاک کردن شماره تلفن یا نام در رمز
                if (!string.IsNullOrEmpty(phoneNumber) && password.Contains(phoneNumber.Replace("09", "").Substring(0, 4)))
                    return (0, "رمز عبور نمی‌تواند شامل شماره تلفن باشد", "danger");

                if (!string.IsNullOrEmpty(fullName) && fullName.Split(' ').Any(part => password.Contains(part, StringComparison.OrdinalIgnoreCase)))
                    return (0, "رمز عبور نمی‌تواند شامل نام شما باشد", "danger");

                // نتیجه نهایی
                if (score >= 85) return (score, "عالی — رمز عبور بسیار قوی", "success");
                if (score >= 70) return (score, "خوب — رمز عبور قوی", "success");
                if (score >= 50) return (score, "متوسط — بهتر است رمز عبور قوی تری انتخاب کنید", "warning");
                return (score, "ضعیف — لطفاً از ترکیب بهتری استفاده کنید", "danger");
            }
        }


        #region Helpers
        // متدهای کمکی Base64UrlSafe
        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            string incoming = input.Replace('-', '+').Replace('_', '/');
            switch (incoming.Length % 4)
            {
                case 2: incoming += "=="; break;
                case 3: incoming += "="; break;
            }
            return Convert.FromBase64String(incoming);
        }
        #endregion
    }
}
