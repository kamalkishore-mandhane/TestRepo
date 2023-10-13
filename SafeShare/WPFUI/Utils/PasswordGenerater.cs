using SafeShare.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SafeShare.WPFUI.Utils
{
    public struct PasswordPolicy
    {
        public int MinLength;
        public bool ReqLowerCharacter;
        public bool ReqUpperCharacter;
        public bool ReqNumericCharacter;
        public bool ReqSymbolCharacter;
    }

    public static class PasswordGenerater
    {
        public static int DefaultPwdMinLength = 8;

        // some characters are excluded for similarity to others, (like i,I,l,L and so on)
        private static string UppercaseAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ";
        private static string LowercaseAlphabet = "abcdefghjkmnpqrstuvwxyz";
        private static string NumCharacterList = "123456789";
        private static string SymbolCharacterList = "!#$%&()*+,-.:;<=>?@{}[]_";
        private static int DefaultGeneratePwdLength = 8;

        public static string GenerateSuggestedPassword()
        {
            return GenerateSuggestedPassword(DefaultGeneratePwdLength);
        }

        public static string GenerateSuggestedPassword(int length)
        {
            var policy = RegeditOperation.GetPasswordPolicy();
            var passwordLength = Math.Max(length, policy.MinLength);
            string password = string.Empty;

            if (policy.ReqLowerCharacter)
            {
                password += GeneratePasswordString(1, LowercaseAlphabet);
            }
            if (policy.ReqUpperCharacter)
            {
                password += GeneratePasswordString(1, UppercaseAlphabet);
            }
            if (policy.ReqNumericCharacter)
            {
                password += GeneratePasswordString(1, NumCharacterList);
            }
            if (policy.ReqSymbolCharacter)
            {
                password += GeneratePasswordString(1, SymbolCharacterList);
            }

            if (passwordLength - password.Length > 0)
            {
                var CharactersList = UppercaseAlphabet + LowercaseAlphabet + NumCharacterList + SymbolCharacterList;
                password += GeneratePasswordString(passwordLength - password.Length, CharactersList);
            }

            return Shuffle(password);
        }

        private static string Shuffle(string str)
        {
            var array = str.ToCharArray();
            var rnd = new Random();
            var newArray = array.OrderBy(x => rnd.Next()).ToArray();
            return new string(newArray);
        }

        public static string GeneratePasswordString(int length, IEnumerable<char> characterSet)
        {
            var characterArray = characterSet.Distinct().ToArray();
            var bytes = new byte[length * 8];
            new RNGCryptoServiceProvider().GetBytes(bytes);
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                ulong value = BitConverter.ToUInt64(bytes, i * 8);
                result[i] = characterArray[value % (uint)characterArray.Length];
            }

            return new string(result);
        }
    }
}