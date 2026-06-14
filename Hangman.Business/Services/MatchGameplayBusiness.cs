using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.UserCases.MatchGameplay.CategoryVoting;
using Hangman.Business.UserCases.MatchGameplay.WordSelection;
using Hangman.Contracts.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class MatchGameplayBusiness : IMatchGameplayBusiness
    {
        private readonly VoteCategoryUseCase voteCategoryUseCase;
        private readonly GetCategoryVotingStateUseCase getCategoryVotingStateUseCase;
        private readonly ResolveCategoryVotingUseCase resolveCategoryVotingUseCase;
        private readonly GetSelectableWordsUseCase getSelectableWordsUseCase;
        private readonly SelectWordUseCase selectWordUseCase;

        public MatchGameplayBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            if (unitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }

            voteCategoryUseCase = new VoteCategoryUseCase(unitOfWorkFactory);
            getCategoryVotingStateUseCase =
                new GetCategoryVotingStateUseCase(unitOfWorkFactory);
            resolveCategoryVotingUseCase =
                new ResolveCategoryVotingUseCase(unitOfWorkFactory);
            getSelectableWordsUseCase =
                new GetSelectableWordsUseCase(unitOfWorkFactory);
            selectWordUseCase = new SelectWordUseCase(unitOfWorkFactory);
        }

        public Task<VoteCategoryResponse> VoteCategoryAsync(
            VoteCategoryRequest request)
        {
            return voteCategoryUseCase.ExecuteAsync(request);
        }

        public Task<GetCategoryVotingStateResponse> GetCategoryVotingStateAsync(
            GetCategoryVotingStateRequest request)
        {
            return getCategoryVotingStateUseCase.ExecuteAsync(request);
        }

        public Task<ResolveCategoryVotingResponse> ResolveCategoryVotingAsync(
            ResolveCategoryVotingRequest request)
        {
            return resolveCategoryVotingUseCase.ExecuteAsync(request);
        }

        public Task<GetSelectableWordsResponse> GetSelectableWordsAsync(
            GetSelectableWordsRequest request)
        {
            return getSelectableWordsUseCase.ExecuteAsync(request);
        }

        public Task<SelectWordResponse> SelectWordAsync(SelectWordRequest request)
        {
            return selectWordUseCase.ExecuteAsync(request);
        }
    }
}
