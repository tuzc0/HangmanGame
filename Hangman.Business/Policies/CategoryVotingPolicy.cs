using Hangman.Business.Constants;
using Hangman.DataAccess.Transporters;
using System;

namespace Hangman.Business.Policies
{
    internal static class CategoryVotingPolicy
    {
        public static bool HasCategoryVotingExpired(MatchTransporter match)
        {
            return match == null ||
                   !match.CategoryVotingEndsAt.HasValue ||
                   DateTime.UtcNow > match.CategoryVotingEndsAt.Value;
        }

        public static int GetRemainingVotingSeconds(MatchTransporter match)
        {
            if (match == null || !match.CategoryVotingEndsAt.HasValue)
            {
                return 0;
            }

            double remainingSeconds =
                (match.CategoryVotingEndsAt.Value - DateTime.UtcNow).TotalSeconds;

            if (remainingSeconds <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(remainingSeconds);
        }

        public static bool CanVote(MatchTransporter match)
        {
            return match != null &&
                   match.MatchStatus == MatchStatusConstants.VotingCategory &&
                   match.CategoryVotingEndsAt.HasValue &&
                   DateTime.UtcNow <= match.CategoryVotingEndsAt.Value;
        }

        public static bool IsVotingResolved(MatchTransporter match)
        {
            return match != null &&
                   match.SelectedCategoryId.HasValue &&
                   match.MatchStatus != MatchStatusConstants.VotingCategory;
        }
    }
}
