using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IMatchGuessRepository
    {
        Task<MatchGuessTransporter> GetByIdAsync(int guessId);

        Task<List<MatchGuessTransporter>> GetByMatchIdAsync(int matchId);

        Task<MatchGuessTransporter> GetByMatchAndLetterAsync(
            int matchId,
            string letter);

        Task<bool> LetterExistsAsync(int matchId, string letter);

        void Add(CreateMatchGuessTransporter guess);
    }
}
