using Hangman.Business.Constants;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.Helpers
{
    internal static class MatchGuessCompletionHelper
    {
        public static bool IsWordCompleted(
            string word,
            IEnumerable<string> correctLetters)
        {
            string normalizedWord = GuessTextNormalizer.Normalize(word);

            if (string.IsNullOrWhiteSpace(normalizedWord))
            {
                return false;
            }

            HashSet<string> guessedLetters = correctLetters == null
                ? new HashSet<string>()
                : new HashSet<string>(
                    correctLetters
                        .Select(GuessTextNormalizer.Normalize)
                        .Where(letter => !string.IsNullOrWhiteSpace(letter)));

            return normalizedWord
                .Where(character => char.IsLetter(character))
                .Select(character => character.ToString())
                .All(letter => guessedLetters.Contains(letter));
        }

        public static async Task<bool> FinishWithGuesserWinAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match)
        {
            if (match == null || !match.GuestId.HasValue)
            {
                return false;
            }

            bool finished = await unitOfWork.Matches.FinishAsync(
                new FinishMatchTransporter
                {
                    MatchId = match.MatchId,
                    WinnerId = match.GuestId.Value,
                    MatchStatus = MatchStatusConstants.Finished
                });

            if (!finished)
            {
                return false;
            }

            unitOfWork.ScoreMovements.Add(
                new CreateScoreMovementTransporter
                {
                    MatchId = match.MatchId,
                    PlayerId = match.GuestId.Value,
                    Points = ScorePointsConstants.GuesserWin,
                    MovementType = ScoreMovementTypeConstants.GuesserWin
                });

            return true;
        }

        public static async Task<bool> FinishWithHostWinAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match)
        {
            if (match == null)
            {
                return false;
            }

            bool finished = await unitOfWork.Matches.FinishAsync(
                new FinishMatchTransporter
                {
                    MatchId = match.MatchId,
                    WinnerId = match.HostId,
                    MatchStatus = MatchStatusConstants.Finished
                });

            if (!finished)
            {
                return false;
            }

            unitOfWork.ScoreMovements.Add(
                new CreateScoreMovementTransporter
                {
                    MatchId = match.MatchId,
                    PlayerId = match.HostId,
                    Points = ScorePointsConstants.HostWin,
                    MovementType = ScoreMovementTypeConstants.HostWin
                });

            return true;
        }
    }
}
