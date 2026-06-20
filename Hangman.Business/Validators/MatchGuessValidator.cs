using Hangman.Business.Messages;
using Hangman.Contracts.Match;

namespace Hangman.Business.Validators
{
    public static class MatchGuessValidator
    {
        public static MatchMessageCode? ValidateGetGameState(
            GetMatchGameStateRequest request)
        {
            if (request == null)
            {
                return MatchMessageCode.InvalidMatchId;
            }

            return ValidateMatchAndAccount(
                request.MatchId,
                request.AccountId);
        }

        public static MatchMessageCode? ValidateGuessLetter(
            GuessLetterRequest request)
        {
            if (request == null)
            {
                return MatchMessageCode.InvalidMatchId;
            }

            MatchMessageCode? basicValidation = ValidateMatchAndAccount(
                request.MatchId,
                request.AccountId);

            if (basicValidation.HasValue)
            {
                return basicValidation.Value;
            }

            string letter = request.Letter?.Trim();

            if (string.IsNullOrWhiteSpace(letter) ||
                letter.Length != 1)
            {
                return MatchMessageCode.InvalidLetter;
            }

            return null;
        }

        public static MatchMessageCode? ValidateGuessWord(
            GuessWordRequest request)
        {
            if (request == null)
            {
                return MatchMessageCode.InvalidMatchId;
            }

            MatchMessageCode? basicValidation = ValidateMatchAndAccount(
                request.MatchId,
                request.AccountId);

            if (basicValidation.HasValue)
            {
                return basicValidation.Value;
            }

            string word = request.Word?.Trim();

            if (string.IsNullOrWhiteSpace(word))
            {
                return MatchMessageCode.InvalidWordGuess;
            }

            return null;
        }

        public static MatchMessageCode? ValidateResolveGuessTimeout(
            ResolveGuessTimeoutRequest request)
        {
            if (request == null)
            {
                return MatchMessageCode.InvalidMatchId;
            }

            return ValidateMatchAndAccount(
                request.MatchId,
                request.AccountId);
        }

        private static MatchMessageCode? ValidateMatchAndAccount(
            int matchId,
            int accountId)
        {
            if (matchId <= 0)
            {
                return MatchMessageCode.InvalidMatchId;
            }

            if (accountId <= 0)
            {
                return MatchMessageCode.InvalidAccountId;
            }

            return null;
        }
    }
}
