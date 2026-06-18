using Hangman.DataAccess.Transporters;

namespace Hangman.Business.Helpers
{
    internal static class GuessEvaluator
    {
        public static string GetWordForPlayer(
    MatchTransporter match,
    int playerId)
        {
            if (match == null)
            {
                return string.Empty;
            }

            if (match.HostId == playerId)
            {
                return match.HostWordText ?? string.Empty;
            }

            if (match.GuestId.HasValue &&
                match.GuestId.Value == playerId)
            {
                return match.GuestWordText ?? string.Empty;
            }

            return string.Empty;
        }

        public static bool ContainsLetter(
            string word,
            string letter)
        {
            string normalizedWord = GuessTextNormalizer.Normalize(word);
            string normalizedLetter = GuessTextNormalizer.Normalize(letter);

            return normalizedWord.Contains(normalizedLetter);
        }

        public static bool WordMatches(
            string word,
            string guessedWord)
        {
            string normalizedWord = GuessTextNormalizer.Normalize(word);
            string normalizedGuessedWord = GuessTextNormalizer.Normalize(guessedWord);

            return normalizedWord == normalizedGuessedWord;
        }
    }
}
