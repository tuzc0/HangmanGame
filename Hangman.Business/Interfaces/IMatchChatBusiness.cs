using Hangman.Contracts.Match;
using System.Threading.Tasks;

namespace Hangman.Business.Interfaces
{
    public interface IMatchChatBusiness
    {
        Task<SendMatchChatMessageResponse> SendMessageAsync(
            SendMatchChatMessageRequest request);
    }
}
