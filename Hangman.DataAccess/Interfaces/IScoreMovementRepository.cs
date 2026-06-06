using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IScoreMovementRepository
    {
        Task<ScoreMovementTransporter> GetByIdAsync(int scoreMovementId);

        Task<List<ScoreMovementTransporter>> GetByPlayerIdAsync(int playerId);

        Task<List<ScoreMovementTransporter>> GetByMatchIdAsync(int matchId);

        Task<List<ScoreMovementTransporter>> GetByPlayerIdAndMovementTypeAsync(
            int playerId,
            string movementType);

        Task<int> GetTotalScoreByPlayerIdAsync(int playerId);

        void Add(CreateScoreMovementTransporter scoreMovement);
    }
}
