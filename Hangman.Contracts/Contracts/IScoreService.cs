using Hangman.Contracts.Match;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Contracts.Contracts
{
    [ServiceContract]
    public interface IScoreService
    {
        [OperationContract]
        Task<GetPlayerScoreResponse> GetPlayerScoreAsync(GetPlayerScoreRequest request);
    }
}
