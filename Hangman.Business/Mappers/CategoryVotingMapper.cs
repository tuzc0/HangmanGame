using Hangman.Contracts.Match;
using Hangman.DataAccess.Transporters;

namespace Hangman.Business.Mappers
{
    internal static class CategoryVotingMapper
    {
        public static CategoryVoteDto ToCategoryVoteDto(
            MatchCategoryVoteTransporter vote)
        {
            if (vote == null)
            {
                return null;
            }

            return new CategoryVoteDto
            {
                PlayerId = vote.PlayerId,
                CategoryId = vote.CategoryId,
                CategoryName = vote.CategoryName,
                LanguageCode = vote.LanguageCode,
                CreatedAt = vote.CreatedAt,
                UpdatedAt = vote.UpdatedAt
            };
        }
    }
}
