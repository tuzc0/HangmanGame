using Hangman.Contracts.Match;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Contracts.Contracts
{
    [ServiceContract]
    public interface IMatchChatService
    {
        [OperationContract]
        Task<SendMatchChatMessageResponse> SendMessageAsync(
            SendMatchChatMessageRequest request);
    }
}
