using System;
using System.Security.Cryptography;

namespace Hangman.Business.Helpers
{
    internal static class RandomCategorySelector
    {
        public static int GetRandomIndex(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxExclusive),
                    "The maximum exclusive value must be greater than zero.");
            }

            using (RandomNumberGenerator randomNumberGenerator =
                RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[4];

                int maxValue = int.MaxValue - (int.MaxValue % maxExclusive);
                int randomValue;

                do
                {
                    randomNumberGenerator.GetBytes(randomBytes);
                    randomValue = BitConverter.ToInt32(randomBytes, 0) & int.MaxValue;
                }
                while (randomValue >= maxValue);

                return randomValue % maxExclusive;
            }
        }
    }
}
