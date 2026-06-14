using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IMatchCategoryVoteRepository
    {
        Task<MatchCategoryVoteTransporter> GetByMatchAndPlayerAsync(
            int matchId,
            int playerId);

        Task<List<MatchCategoryVoteTransporter>> GetByMatchIdAsync(int matchId);

        Task<List<MatchCategoryVoteTransporter>> GetByMatchIdAsync(
            int matchId,
            string languageCode);

        Task<bool> UpsertAsync(SaveMatchCategoryVoteTransporter vote);
    }
}
