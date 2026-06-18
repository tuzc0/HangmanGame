using Hangman.Contracts.Match;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Contracts.Contracts
{
    [ServiceContract]
    public interface IMatchGuessService
    {
        [OperationContract]
        Task<GetMatchGameStateResponse> GetGameStateAsync(
            GetMatchGameStateRequest request);

        [OperationContract]
        Task<GuessLetterResponse> GuessLetterAsync(
            GuessLetterRequest request);

        [OperationContract]
        Task<GuessWordResponse> GuessWordAsync(
            GuessWordRequest request);

        [OperationContract]
        Task<ResolveGuessTimeoutResponse> ResolveGuessTimeoutAsync(
            ResolveGuessTimeoutRequest request);
    }
}
