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
    internal class GetMatchGameStateUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public GetMatchGameStateUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<GetMatchGameStateResponse> ExecuteAsync(
            GetMatchGameStateRequest request)
        {
            if (request == null)
            {
                return MatchGuessResponseFactory.BuildGetMatchGameStateResponse(
                    false,
                    MatchMessageCode.InvalidMatchId,
                    null);
            }

            MatchMessageCode? validationResult =
                MatchGuessValidator.ValidateGetGameState(request);

            if (validationResult.HasValue)
            {
                return MatchGuessResponseFactory.BuildGetMatchGameStateResponse(
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
                    return MatchGuessResponseFactory.BuildGetMatchGameStateResponse(
                        false,
                        playerAvailability.MessageCode,
                        null);
                }

                MatchTransporter match =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                MatchMessageCode? accessValidation =
                    GuessAccessPolicy.ValidateGameStateAccess(
                        match,
                        playerAvailability.Player.PlayerId);

                if (accessValidation.HasValue)
                {
                    return MatchGuessResponseFactory.BuildGetMatchGameStateResponse(
                        false,
                        accessValidation.Value,
                        null);
                }

                MatchGameStateDto gameState =
                    await MatchGameStateLoader.BuildAsync(
                        unitOfWork,
                        match,
                        playerAvailability.Player.PlayerId);

                return MatchGuessResponseFactory.BuildGetMatchGameStateResponse(
                    true,
                    MatchMessageCode.GameStateRetrieved,
                    gameState);
            }
        }
    }
}
