using Hangman.Contracts.Match;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Contracts.Contracts
{
    [ServiceContract(CallbackContract = typeof(IMatchNotificationCallback))]
    public interface IMatchNotificationService
    {
        [OperationContract]
        Task<SubscribeMatchResponse> SubscribeAsync(SubscribeMatchRequest request);

        [OperationContract]
        Task<UnsubscribeMatchResponse> UnsubscribeAsync(UnsubscribeMatchRequest request);

        [OperationContract]
        Task<SubscribeAvailableLobbiesResponse> SubscribeAvailableLobbiesAsync(
            SubscribeAvailableLobbiesRequest request);

        [OperationContract]
        Task<UnsubscribeAvailableLobbiesResponse> UnsubscribeAvailableLobbiesAsync(
            UnsubscribeAvailableLobbiesRequest request);
    }
}
