using System;

namespace Hangman.Business.Helpers
{
    internal static class RandomCategorySelector
    {
        private static readonly object RandomSyncRoot = new object();
        private static readonly Random RandomGenerator = new Random();

        public static int GetRandomIndex(int maxExclusive)
        {
            lock (RandomSyncRoot)
            {
                return RandomGenerator.Next(maxExclusive);
            }
        }
    }
}
