using Hangman.Business.Constants;
using Hangman.Business.Factories;
using Hangman.Business.Helpers;
using Hangman.Business.Mappers;
using Hangman.Business.Messages;
using Hangman.Business.Policies;
using Hangman.Business.Results;
using Hangman.Business.Validators;
using Hangman.Contracts.Match;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.MatchGameplay.CategoryVoting
{
    internal class GetCategoryVotingStateUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public GetCategoryVotingStateUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<GetCategoryVotingStateResponse> ExecuteAsync(
            GetCategoryVotingStateRequest request)
        {
            MatchMessageCode? validationResult =
                MatchGameplayValidator.ValidateGetCategoryVotingState(request);

            if (validationResult.HasValue)
            {
                return MatchGameplayResponseFactory.BuildGetCategoryVotingStateResponse(
                    false,
                    validationResult.Value,
                    null);
            }

            using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await PlayerAvailabilityPolicy.ValidateForMatchAsync(
                        unitOfWork,
                        request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return MatchGameplayResponseFactory.BuildGetCategoryVotingStateResponse(
                        false,
                        playerAvailability.MessageCode,
                        null);
                }

                MatchTransporter match =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                MatchMessageCode? matchValidation =
                    MatchAccessPolicy.ValidateMatchForPlayer(
                        match,
                        playerAvailability.Player.PlayerId);

                if (matchValidation.HasValue)
                {
                    return MatchGameplayResponseFactory.BuildGetCategoryVotingStateResponse(
                        false,
                        matchValidation.Value,
                        null);
                }

                MatchMessageCode messageCode =
                    MatchMessageCode.CategoryVotingStateRetrieved;

                if (match.MatchStatus == MatchStatusConstants.VotingCategory &&
                    CategoryVotingPolicy.HasCategoryVotingExpired(match))
                {
                    MatchTransporter resolvedMatch =
                        await CategoryVotingResolver.ResolveAsync(unitOfWork, match);

                    if (resolvedMatch == null)
                    {
                        return MatchGameplayResponseFactory.BuildGetCategoryVotingStateResponse(
                            false,
                            MatchMessageCode.CategoryVotingResolveFailed,
                            null);
                    }

                    await unitOfWork.CommitAsync();

                    match = resolvedMatch;
                    messageCode = MatchMessageCode.CategoryVotingResolved;
                }

                CategoryVotingStateDto votingState =
                    await CategoryVotingStateBuilder.BuildAsync(
                        unitOfWork,
                        match,
                        playerAvailability.Player.PlayerId);

                return MatchGameplayResponseFactory.BuildGetCategoryVotingStateResponse(
                    true,
                    messageCode,
                    votingState);
            }
        }
    }
}
