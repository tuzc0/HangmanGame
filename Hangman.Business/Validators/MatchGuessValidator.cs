using Hangman.Business.Messages;
using Hangman.Contracts.Match;

namespace Hangman.Business.Validators
{
    public static class MatchGuessValidator
    {
        public static MatchMessageCode? ValidateGetGameState(
            GetMatchGameStateRequest request)
        {
            return ValidateMatchAndAccount(
                request == null ? 0 : request.MatchId,
                request == null ? 0 : request.AccountId);
        }

        public static MatchMessageCode? ValidateGuessLetter(
            GuessLetterRequest request)
        {
            MatchMessageCode? basicValidation = ValidateMatchAndAccount(
                request == null ? 0 : request.MatchId,
                request == null ? 0 : request.AccountId);

            if (basicValidation.HasValue)
            {
                return basicValidation.Value;
            }

            if (string.IsNullOrWhiteSpace(request.Letter) ||
                request.Letter.Trim().Length != 1)
            {
                return MatchMessageCode.InvalidLetter;
            }

            return null;
        }

        public static MatchMessageCode? ValidateGuessWord(
            GuessWordRequest request)
        {
            MatchMessageCode? basicValidation = ValidateMatchAndAccount(
                request == null ? 0 : request.MatchId,
                request == null ? 0 : request.AccountId);

            if (basicValidation.HasValue)
            {
                return basicValidation.Value;
            }

            if (string.IsNullOrWhiteSpace(request.Word))
            {
                return MatchMessageCode.InvalidWordGuess;
            }

            return null;
        }

        public static MatchMessageCode? ValidateResolveGuessTimeout(
            ResolveGuessTimeoutRequest request)
        {
            return ValidateMatchAndAccount(
                request == null ? 0 : request.MatchId,
                request == null ? 0 : request.AccountId);
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
