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

namespace Hangman.Business.UserCases.MatchGuess
{
    internal class ResolveGuessTimeoutUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public ResolveGuessTimeoutUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<ResolveGuessTimeoutResponse> ExecuteAsync(
            ResolveGuessTimeoutRequest request)
        {
            if (request == null)
            {
                return MatchGuessResponseFactory.BuildResolveGuessTimeoutResponse(
                    false,
                    MatchMessageCode.InvalidMatchId,
                    false,
                    null);
            }

            MatchMessageCode? validationResult =
                MatchGuessValidator.ValidateResolveGuessTimeout(request);

            if (validationResult.HasValue)
            {
                return MatchGuessResponseFactory.BuildResolveGuessTimeoutResponse(
                    false,
                    validationResult.Value,
                    false,
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
                    return MatchGuessResponseFactory
                        .BuildResolveGuessTimeoutResponse(
                            false,
                            playerAvailability.MessageCode,
                            false,
                            null);
                }

                int playerId = playerAvailability.Player.PlayerId;

                MatchTransporter match =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                MatchMessageCode? accessValidation =
                    GuessAccessPolicy.ValidateGameStateAccess(match, playerId);

                if (accessValidation.HasValue)
                {
                    return MatchGuessResponseFactory
                        .BuildResolveGuessTimeoutResponse(
                            false,
                            accessValidation.Value,
                            false,
                            null);
                }

                MatchGameStateDto currentState =
                    await MatchGameStateLoader.BuildAsync(
                        unitOfWork,
                        match,
                        playerId);

                if (match.MatchStatus != MatchStatusConstants.InProgress)
                {
                    return MatchGuessResponseFactory
                        .BuildResolveGuessTimeoutResponse(
                            true,
                            MatchMessageCode.MatchFinished,
                            true,
                            currentState);
                }

                if (!GuessTurnPolicy.HasGuessTurnStarted(match))
                {
                    return MatchGuessResponseFactory
                        .BuildResolveGuessTimeoutResponse(
                            false,
                            MatchMessageCode.GuessTurnNotStarted,
                            false,
                            currentState);
                }

                if (!GuessTurnPolicy.HasGuessTurnExpired(match))
                {
                    return MatchGuessResponseFactory
                        .BuildResolveGuessTimeoutResponse(
                            true,
                            MatchMessageCode.GuessTurnStillActive,
                            false,
                            currentState);
                }

                bool timeoutResolved =
                    await MatchGuessCompletionHelper.FinishWithHostWinAsync(
                        unitOfWork,
                        match);

                if (!timeoutResolved)
                {
                    return MatchGuessResponseFactory
                        .BuildResolveGuessTimeoutResponse(
                            false,
                            MatchMessageCode.UnexpectedError,
                            false,
                            currentState);
                }

                await unitOfWork.CommitAsync();

                MatchGameStateDto updatedState =
                    await MatchGameStateLoader.BuildByMatchIdAsync(
                        unitOfWork,
                        match.MatchId,
                        playerId);

                return MatchGuessResponseFactory
                    .BuildResolveGuessTimeoutResponse(
                        true,
                        MatchMessageCode.GuessTimeoutResolved,
                        true,
                        updatedState);
            }
        }
    }
}
