using Hangman.Contracts.Match;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Contracts.Contracts
{
    [ServiceContract]
    public interface IMatchService
    {
        [OperationContract]
        Task<CreateLobbyResponse> CreateLobbyAsync(CreateLobbyRequest request);

        [OperationContract]
        Task<GetAvailableLobbiesResponse> GetAvailableLobbiesAsync(
            GetAvailableLobbiesRequest request);

        [OperationContract]
        Task<JoinLobbyResponse> JoinLobbyAsync(JoinLobbyRequest request);

        [OperationContract]
        Task<GetCurrentLobbyResponse> GetCurrentLobbyAsync(
            GetCurrentLobbyRequest request);

        [OperationContract]
        Task<LeaveLobbyResponse> LeaveLobbyAsync(LeaveLobbyRequest request);
    }
}
