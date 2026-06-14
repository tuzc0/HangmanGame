using Hangman.Business.Factories;
using Hangman.Business.Helpers;
using Hangman.Business.Mappers;
using Hangman.Business.Messages;
using Hangman.Business.Policies;
using Hangman.Business.Results;
using Hangman.Business.Validators;
using Hangman.Contracts.Match;
using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.Match
{
    internal class LeaveLobbyUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public LeaveLobbyUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<LeaveLobbyResponse> ExecuteAsync(LeaveLobbyRequest request)
        {
            MatchMessageCode? validationResult =
                MatchValidator.ValidateLeaveLobby(request);

            if (validationResult.HasValue)
            {
                return MatchResponseFactory.BuildLeaveLobbyResponse(
                    false,
                    validationResult.Value);
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await PlayerAvailabilityPolicy.ValidateForMatchAsync(
                        unitOfWork,
                        request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return MatchResponseFactory.BuildLeaveLobbyResponse(
                        false,
                        playerAvailability.MessageCode);
                }

                MatchTransporter match =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                if (match == null)
                {
                    return MatchResponseFactory.BuildLeaveLobbyResponse(
                        false,
                        MatchMessageCode.MatchNotFound);
                }

                bool playerBelongsToMatch =
                    LobbyLeavePolicy.PlayerBelongsToMatch(
                        match,
                        playerAvailability.Player.PlayerId);

                if (!playerBelongsToMatch)
                {
                    return MatchResponseFactory.BuildLeaveLobbyResponse(
                        false,
                        MatchMessageCode.LobbyLeaveNotAllowed);
                }

                if (!ActiveMatchPolicy.IsActiveMatch(match))
                {
                    return MatchResponseFactory.BuildLeaveLobbyResponse(
                        false,
                        MatchMessageCode.MatchAlreadyResolved);
                }

                if (LobbyLeavePolicy.CanLeaveWithoutPenalty(match))
                {
                    bool finished =
                        await MatchCompletionHelper.FinishMatchWithoutPenaltyAsync(
                            unitOfWork,
                            match.MatchId);

                    if (!finished)
                    {
                        return MatchResponseFactory.BuildLeaveLobbyResponse(
                            false,
                            MatchMessageCode.LobbyLeaveFailed);
                    }

                    await unitOfWork.CommitAsync();

                    return MatchResponseFactory.BuildLeaveLobbyResponse(
                        true,
                        MatchMessageCode.LobbyLeft);
                }

                int penalizedPlayerId = playerAvailability.Player.PlayerId;

                int winnerPlayerId = MatchWinnerResolver.GetWinnerPlayerId(
                    match,
                    penalizedPlayerId);

                if (winnerPlayerId <= 0)
                {
                    return MatchResponseFactory.BuildLeaveLobbyResponse(
                        false,
                        MatchMessageCode.LobbyLeaveFailed);
                }

                bool abandoned =
                    await MatchCompletionHelper.RegisterPenalizedAbandonAsync(
                        unitOfWork,
                        match.MatchId,
                        penalizedPlayerId,
                        winnerPlayerId);

                if (!abandoned)
                {
                    return MatchResponseFactory.BuildLeaveLobbyResponse(
                        false,
                        MatchMessageCode.LobbyLeaveFailed);
                }

                await unitOfWork.CommitAsync();

                return MatchResponseFactory.BuildLeaveLobbyResponse(
                    true,
                    MatchMessageCode.LobbyAbandoned);
            }
        }
    }
}
