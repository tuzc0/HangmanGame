using Hangman.Business.Configuration;
using System;
using System.Security.Cryptography;

namespace Hangman.Business.Security
{
    public class VerificationCodeGenerator
    {
        public string GenerateCode(AuthSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            byte[] bytes = new byte[4];

            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(bytes);
            }

            uint value = BitConverter.ToUInt32(bytes, 0);
            int code = (int)(value % settings.VerificationCodeLimit);

            return code.ToString().PadLeft(settings.VerificationCodeLength, '0');
        }
    }
}
