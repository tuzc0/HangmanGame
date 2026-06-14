using Hangman.Business.Messages;
using Hangman.Contracts.Match;

namespace Hangman.Business.Validators
{
    public static class MatchGameplayValidator
    {
        public static MatchMessageCode? ValidateVoteCategory(
            VoteCategoryRequest request)
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

            if (request.CategoryId <= 0)
            {
                return MatchMessageCode.InvalidCategoryId;
            }

            return null;
        }

        public static MatchMessageCode? ValidateGetCategoryVotingState(
            GetCategoryVotingStateRequest request)
        {
            if (request == null)
            {
                return MatchMessageCode.InvalidMatchId;
            }

            return ValidateMatchAndAccount(
                request.MatchId,
                request.AccountId);
        }

        public static MatchMessageCode? ValidateResolveCategoryVoting(
            ResolveCategoryVotingRequest request)
        {
            if (request == null)
            {
                return MatchMessageCode.InvalidMatchId;
            }

            return ValidateMatchAndAccount(
                request.MatchId,
                request.AccountId);
        }

        public static MatchMessageCode? ValidateGetSelectableWords(
            GetSelectableWordsRequest request)
        {
            if (request == null)
            {
                return MatchMessageCode.InvalidMatchId;
            }

            return ValidateMatchAndAccount(
                request.MatchId,
                request.AccountId);
        }

        public static MatchMessageCode? ValidateSelectWord(
            SelectWordRequest request)
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

            if (request.WordId <= 0)
            {
                return MatchMessageCode.InvalidWordId;
            }

            return null;
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
