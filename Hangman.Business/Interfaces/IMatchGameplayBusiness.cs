using Hangman.Contracts.Match;
using System.Threading.Tasks;

namespace Hangman.Business.Interfaces
{
    public interface IMatchGameplayBusiness
    {
        Task<VoteCategoryResponse> VoteCategoryAsync(VoteCategoryRequest request);

        Task<GetCategoryVotingStateResponse> GetCategoryVotingStateAsync(
            GetCategoryVotingStateRequest request);

        Task<ResolveCategoryVotingResponse> ResolveCategoryVotingAsync(
            ResolveCategoryVotingRequest request);

        Task<GetSelectableWordsResponse> GetSelectableWordsAsync(
            GetSelectableWordsRequest request);

        Task<SelectWordResponse> SelectWordAsync(SelectWordRequest request);
    }
}
