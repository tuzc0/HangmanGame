using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IMatchRepository
    {
        Task<MatchTransporter> GetByIdAsync(int matchId);

        Task<List<MatchTransporter>> GetByPlayerIdAsync(int playerId);

        Task<List<MatchTransporter>> GetByStatusAsync(string matchStatus);

        Task<List<AvailableMatchTransporter>> GetAvailableByLanguageAsync(
            string matchStatus,
            string languageCode);

        Task<bool> ExistsAsync(int matchId);

        Task<bool> IsPlayerInMatchAsync(int matchId, int playerId);

        void Add(CreateMatchTransporter match);

        Task<bool> JoinAsync(JoinMatchTransporter match);

        Task<bool> IncrementFailedAttemptsAsync(int matchId);

        Task<bool> UpdateStatusAsync(UpdateMatchStatusTransporter match);

        Task<bool> FinishAsync(FinishMatchTransporter match);

        Task<bool> RegisterAbandonAsync(AbandonMatchTransporter match);
    }
}
