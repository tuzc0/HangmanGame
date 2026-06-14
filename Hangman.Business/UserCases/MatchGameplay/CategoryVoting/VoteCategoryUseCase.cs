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
    internal class VoteCategoryUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public VoteCategoryUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<VoteCategoryResponse> ExecuteAsync(
            VoteCategoryRequest request)
        {
            MatchMessageCode? validationResult =
                MatchGameplayValidator.ValidateVoteCategory(request);

            if (validationResult.HasValue)
            {
                return MatchGameplayResponseFactory.BuildVoteCategoryResponse(
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
                    return MatchGameplayResponseFactory.BuildVoteCategoryResponse(
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
                    return MatchGameplayResponseFactory.BuildVoteCategoryResponse(
                        false,
                        matchValidation.Value,
                        null);
                }

                if (match.MatchStatus != MatchStatusConstants.VotingCategory)
                {
                    CategoryVotingStateDto currentState =
                        await CategoryVotingStateBuilder.BuildAsync(
                            unitOfWork,
                            match,
                            playerAvailability.Player.PlayerId);

                    return MatchGameplayResponseFactory.BuildVoteCategoryResponse(
                        false,
                        MatchMessageCode.CategoryVotingNotActive,
                        currentState);
                }

                if (CategoryVotingPolicy.HasCategoryVotingExpired(match))
                {
                    MatchTransporter resolvedMatch =
                        await CategoryVotingResolver.ResolveAsync(unitOfWork, match);

                    if (resolvedMatch == null)
                    {
                        return MatchGameplayResponseFactory.BuildVoteCategoryResponse(
                            false,
                            MatchMessageCode.CategoryVotingResolveFailed,
                            null);
                    }

                    await unitOfWork.CommitAsync();

                    CategoryVotingStateDto resolvedState =
                        await CategoryVotingStateBuilder.BuildAsync(
                            unitOfWork,
                            resolvedMatch,
                            playerAvailability.Player.PlayerId);

                    return MatchGameplayResponseFactory.BuildVoteCategoryResponse(
                        false,
                        MatchMessageCode.CategoryVotingExpired,
                        resolvedState);
                }

                bool categoryExists =
                    await unitOfWork.Words.ExistsActiveCategoryAsync(
                        request.CategoryId);

                if (!categoryExists)
                {
                    return MatchGameplayResponseFactory.BuildVoteCategoryResponse(
                        false,
                        MatchMessageCode.CategoryNotAvailable,
                        null);
                }

                MatchCategoryVoteTransporter existingVote =
                    await unitOfWork.MatchCategoryVotes.GetByMatchAndPlayerAsync(
                        request.MatchId,
                        playerAvailability.Player.PlayerId);

                bool saved = await unitOfWork.MatchCategoryVotes.UpsertAsync(
                    new SaveMatchCategoryVoteTransporter
                    {
                        MatchId = request.MatchId,
                        PlayerId = playerAvailability.Player.PlayerId,
                        CategoryId = request.CategoryId
                    });

                if (!saved)
                {
                    return MatchGameplayResponseFactory.BuildVoteCategoryResponse(
                        false,
                        MatchMessageCode.UnexpectedError,
                        null);
                }

                await unitOfWork.CommitAsync();

                MatchTransporter updatedMatch =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                CategoryVotingStateDto votingState =
                    await CategoryVotingStateBuilder.BuildAsync(
                        unitOfWork,
                        updatedMatch,
                        playerAvailability.Player.PlayerId);

                MatchMessageCode messageCode = existingVote == null
                    ? MatchMessageCode.CategoryVoteRegistered
                    : MatchMessageCode.CategoryVoteUpdated;

                return MatchGameplayResponseFactory.BuildVoteCategoryResponse(
                    true,
                    messageCode,
                    votingState);
            }
        }
    }
}
