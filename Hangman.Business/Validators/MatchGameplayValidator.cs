using Hangman.Business.Messages;
using Hangman.Contracts.Match;

namespace Hangman.Business.Validators
{
    public static class MatchGameplayValidator
    {
        public static MatchMessageCode? ValidateVoteCategory(
            VoteCategoryRequest request)
        {
            MatchMessageCode? basicValidation = ValidateMatchAndAccount(
                request == null ? 0 : request.MatchId,
                request == null ? 0 : request.AccountId);

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
            return ValidateMatchAndAccount(
                request == null ? 0 : request.MatchId,
                request == null ? 0 : request.AccountId);
        }

        public static MatchMessageCode? ValidateResolveCategoryVoting(
            ResolveCategoryVotingRequest request)
        {
            return ValidateMatchAndAccount(
                request == null ? 0 : request.MatchId,
                request == null ? 0 : request.AccountId);
        }

        public static MatchMessageCode? ValidateGetSelectableWords(
            GetSelectableWordsRequest request)
        {
            return ValidateMatchAndAccount(
                request == null ? 0 : request.MatchId,
                request == null ? 0 : request.AccountId);
        }

        public static MatchMessageCode? ValidateSelectWord(
            SelectWordRequest request)
        {
            MatchMessageCode? basicValidation = ValidateMatchAndAccount(
                request == null ? 0 : request.MatchId,
                request == null ? 0 : request.AccountId);

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
