using Hangman.Contracts.Match;
using System.Threading.Tasks;

namespace Hangman.Business.Interfaces
{
    public interface IMatchBusiness
    {
        Task<CreateLobbyResponse> CreateLobbyAsync(CreateLobbyRequest request);

        Task<GetAvailableLobbiesResponse> GetAvailableLobbiesAsync(
            GetAvailableLobbiesRequest request);

        Task<JoinLobbyResponse> JoinLobbyAsync(JoinLobbyRequest request);

        Task<GetCurrentLobbyResponse> GetCurrentLobbyAsync(
            GetCurrentLobbyRequest request);

        Task<LeaveLobbyResponse> LeaveLobbyAsync(LeaveLobbyRequest request);
    }
}
