using Hangman.Business.Mappers;
using Hangman.Business.Policies;
using Hangman.Contracts.Match;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.Helpers
{
    internal static class CategoryVotingStateBuilder
    {
        public static async Task<CategoryVotingStateDto> BuildAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match,
            int currentPlayerId)
        {
            if (match == null)
            {
                return null;
            }

            string languageCode = GetCurrentPlayerLanguageCode(
                match,
                currentPlayerId);

            List<MatchCategoryVoteTransporter> votes =
                await unitOfWork.MatchCategoryVotes.GetByMatchIdAsync(
                    match.MatchId,
                    languageCode);

            return new CategoryVotingStateDto
            {
                MatchId = match.MatchId,
                MatchStatus = match.MatchStatus,
                SelectedCategoryId = match.SelectedCategoryId,
                SelectedCategoryName = GetSelectedCategoryName(
                    match,
                    currentPlayerId),
                CategoryVotingStartedAt = match.CategoryVotingStartedAt,
                CategoryVotingEndsAt = match.CategoryVotingEndsAt,
                WordSelectionStartedAt = match.WordSelectionStartedAt,
                WordSelectionEndsAt = match.WordSelectionEndsAt,
                RemainingVotingSeconds =
                    CategoryVotingPolicy.GetRemainingVotingSeconds(match),
                CanVote = CategoryVotingPolicy.CanVote(match),
                IsVotingResolved = CategoryVotingPolicy.IsVotingResolved(match),
                CanCurrentPlayerSelectWord =
                    WordSelectionPolicy.CanCurrentPlayerSelectWord(
                        match,
                        currentPlayerId),
                Votes = votes
                    .Select(CategoryVotingMapper.ToCategoryVoteDto)
                    .Where(vote => vote != null)
                    .ToList()
            };
        }

        private static string GetCurrentPlayerLanguageCode(
            MatchTransporter match,
            int currentPlayerId)
        {
            if (match.HostId == currentPlayerId)
            {
                return match.HostLanguageCode;
            }

            return match.GuestLanguageCode;
        }

        private static string GetSelectedCategoryName(
            MatchTransporter match,
            int currentPlayerId)
        {
            if (match.HostId == currentPlayerId)
            {
                return match.HostCategoryName;
            }

            return match.GuestCategoryName;
        }
    }
}
