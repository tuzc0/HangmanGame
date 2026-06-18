using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IMatchWordGuessRepository
    {
        Task<MatchWordGuessTransporter> GetByIdAsync(int wordGuessId);

        Task<List<MatchWordGuessTransporter>> GetByMatchIdAsync(int matchId);

        void Add(CreateMatchWordGuessTransporter guess);
    }
}
