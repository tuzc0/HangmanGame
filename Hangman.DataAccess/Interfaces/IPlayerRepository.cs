using Hangman.DataAccess.Transporters;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IPlayerRepository
    {
        Task<PlayerTransporter> GetByIdAsync(int playerId);

        Task<PlayerTransporter> GetByAccountIdAsync(int accountId);

        Task<bool> ExistsAsync(int playerId);

        void Add(CreatePlayerTransporter player);

        Task<bool> UpdateProfileAsync(UpdatePlayerProfileTransporter player);

        Task<bool> UpdateActiveStatusAsync(UpdatePlayerActiveStatusTransporter player);
    }
}
