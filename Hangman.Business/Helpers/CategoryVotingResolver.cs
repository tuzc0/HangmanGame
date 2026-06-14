using Hangman.Business.Constants;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Transporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.Helpers
{
    internal static class CategoryVotingResolver
    {
        public static async Task<MatchTransporter> ResolveAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match)
        {
            if (match == null ||
                match.MatchStatus != MatchStatusConstants.VotingCategory)
            {
                return match;
            }

            List<MatchCategoryVoteTransporter> votes =
                await unitOfWork.MatchCategoryVotes.GetByMatchIdAsync(match.MatchId);

            int selectedCategoryId =
                await ResolveSelectedCategoryIdAsync(unitOfWork, match, votes);

            if (selectedCategoryId <= 0)
            {
                return null;
            }

            DateTime currentDate = DateTime.UtcNow;
            DateTime wordSelectionEndsAt = currentDate.AddSeconds(
                MatchTimingConstants.HostWordSelectionDurationSeconds);

            bool updated = await unitOfWork.Matches.UpdateSelectedCategoryAsync(
                new SelectMatchCategoryTransporter
                {
                    MatchId = match.MatchId,
                    SelectedCategoryId = selectedCategoryId,
                    MatchStatus = MatchStatusConstants.WaitingForHostWord,
                    WordSelectionStartedAt = currentDate,
                    WordSelectionEndsAt = wordSelectionEndsAt
                });

            if (!updated)
            {
                return null;
            }

            match.SelectedCategoryId = selectedCategoryId;
            match.MatchStatus = MatchStatusConstants.WaitingForHostWord;
            match.WordSelectionStartedAt = currentDate;
            match.WordSelectionEndsAt = wordSelectionEndsAt;

            return match;
        }

        private static async Task<int> ResolveSelectedCategoryIdAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match,
            List<MatchCategoryVoteTransporter> votes)
        {
            if (votes != null && votes.Count > 0)
            {
                List<int> votedCategoryIds = votes
                    .Select(vote => vote.CategoryId)
                    .Distinct()
                    .ToList();

                if (votedCategoryIds.Count == 1)
                {
                    return votedCategoryIds[0];
                }

                return votedCategoryIds[
                    RandomCategorySelector.GetRandomIndex(votedCategoryIds.Count)];
            }

            List<CategoryTransporter> categories =
                await unitOfWork.Words.GetActiveCategoriesByLanguageAsync(
                    match.HostLanguageCode);

            if (categories == null || categories.Count == 0)
            {
                return 0;
            }

            return categories[
                RandomCategorySelector.GetRandomIndex(categories.Count)].CategoryId;
        }
    }
}
