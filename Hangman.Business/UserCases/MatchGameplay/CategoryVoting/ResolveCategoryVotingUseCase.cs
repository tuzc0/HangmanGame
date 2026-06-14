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
    internal class ResolveCategoryVotingUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public ResolveCategoryVotingUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<ResolveCategoryVotingResponse> ExecuteAsync(
            ResolveCategoryVotingRequest request)
        {
            MatchMessageCode? validationResult =
                MatchGameplayValidator.ValidateResolveCategoryVoting(request);

            if (validationResult.HasValue)
            {
                return MatchGameplayResponseFactory.BuildResolveCategoryVotingResponse(
                    false,
                    validationResult.Value,
                    null,
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
                    return MatchGameplayResponseFactory.BuildResolveCategoryVotingResponse(
                        false,
                        playerAvailability.MessageCode,
                        null,
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
                    return MatchGameplayResponseFactory.BuildResolveCategoryVotingResponse(
                        false,
                        matchValidation.Value,
                        null,
                        null);
                }

                if (match.MatchStatus != MatchStatusConstants.VotingCategory ||
                    !CategoryVotingPolicy.HasCategoryVotingExpired(match))
                {
                    CategoryVotingStateDto currentState =
                        await CategoryVotingStateBuilder.BuildAsync(
                            unitOfWork,
                            match,
                            playerAvailability.Player.PlayerId);

                    return MatchGameplayResponseFactory.BuildResolveCategoryVotingResponse(
                        true,
                        MatchMessageCode.CategoryVotingStateRetrieved,
                        MatchMapper.ToMatchLobbyDto(match),
                        currentState);
                }

                MatchTransporter resolvedMatch =
                    await CategoryVotingResolver.ResolveAsync(unitOfWork, match);

                if (resolvedMatch == null)
                {
                    return MatchGameplayResponseFactory.BuildResolveCategoryVotingResponse(
                        false,
                        MatchMessageCode.CategoryVotingResolveFailed,
                        null,
                        null);
                }

                await unitOfWork.CommitAsync();

                MatchTransporter updatedMatch =
                    await unitOfWork.Matches.GetByIdAsync(match.MatchId);

                CategoryVotingStateDto votingState =
                    await CategoryVotingStateBuilder.BuildAsync(
                        unitOfWork,
                        updatedMatch,
                        playerAvailability.Player.PlayerId);

                return MatchGameplayResponseFactory.BuildResolveCategoryVotingResponse(
                    true,
                    MatchMessageCode.CategoryVotingResolved,
                    MatchMapper.ToMatchLobbyDto(updatedMatch),
                    votingState);
            }
        }
    }
}
