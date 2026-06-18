using Hangman.Contracts.Match;
using System.Collections.Generic;
using System.Linq;

namespace Hangman.Business.Helpers
{
    internal static class WordProgressBuilder
    {
        public static List<LetterSlotDto> Build(
            string word,
            IEnumerable<string> guessedLetters,
            bool revealAll)
        {
            List<LetterSlotDto> slots = new List<LetterSlotDto>();

            if (string.IsNullOrWhiteSpace(word))
            {
                return slots;
            }

            List<string> normalizedGuessedLetters = guessedLetters == null
                ? new List<string>()
                : guessedLetters
                    .Select(GuessTextNormalizer.Normalize)
                    .Where(letter => !string.IsNullOrWhiteSpace(letter))
                    .ToList();

            for (int index = 0; index < word.Length; index++)
            {
                string currentLetter = word[index].ToString();
                string normalizedLetter = GuessTextNormalizer.Normalize(currentLetter);

                bool isRevealed = revealAll ||
                                  !char.IsLetter(word[index]) ||
                                  normalizedGuessedLetters.Contains(normalizedLetter);

                slots.Add(new LetterSlotDto
                {
                    Position = index,
                    Letter = isRevealed ? currentLetter.ToUpperInvariant() : string.Empty,
                    IsRevealed = isRevealed
                });
            }

            return slots;
        }
    }
}
