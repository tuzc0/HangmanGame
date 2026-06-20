using Hangman.Contracts.Match;
using System.Threading.Tasks;

namespace Hangman.Business.Interfaces
{
    public interface IScoreBusiness
    {
        Task<GetPlayerScoreResponse> GetPlayerScoreAsync(GetPlayerScoreRequest request);
    }
}
