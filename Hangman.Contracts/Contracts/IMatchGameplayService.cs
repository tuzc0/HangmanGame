using Hangman.Contracts.Match;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Contracts.Contracts
{
    [ServiceContract]
    public interface IMatchGameplayService
    {
        [OperationContract]
        Task<VoteCategoryResponse> VoteCategoryAsync(VoteCategoryRequest request);

        [OperationContract]
        Task<GetCategoryVotingStateResponse> GetCategoryVotingStateAsync(
            GetCategoryVotingStateRequest request);

        [OperationContract]
        Task<ResolveCategoryVotingResponse> ResolveCategoryVotingAsync(
            ResolveCategoryVotingRequest request);

        [OperationContract]
        Task<GetSelectableWordsResponse> GetSelectableWordsAsync(
            GetSelectableWordsRequest request);

        [OperationContract]
        Task<SelectWordResponse> SelectWordAsync(SelectWordRequest request);
    }
}
