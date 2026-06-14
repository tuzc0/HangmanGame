using Hangman.Business.Factories;
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
    internal class GetCurrentLobbyUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public GetCurrentLobbyUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<GetCurrentLobbyResponse> ExecuteAsync(
            GetCurrentLobbyRequest request)
        {
            MatchMessageCode? validationResult =
                MatchValidator.ValidateGetCurrentLobby(request);

            if (validationResult.HasValue)
            {
                return MatchResponseFactory.BuildGetCurrentLobbyResponse(
                    false,
                    validationResult.Value,
                    null);
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await PlayerAvailabilityPolicy.ValidateForMatchAsync(
                        unitOfWork,
                        request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return MatchResponseFactory.BuildGetCurrentLobbyResponse(
                        false,
                        playerAvailability.MessageCode,
                        null);
                }

                MatchTransporter currentMatch =
                    await ActiveMatchPolicy.GetCurrentActiveMatchAsync(
                        unitOfWork,
                        playerAvailability.Player.PlayerId);

                if (currentMatch == null)
                {
                    return MatchResponseFactory.BuildGetCurrentLobbyResponse(
                        true,
                        MatchMessageCode.NoActiveLobby,
                        null);
                }

                return MatchResponseFactory.BuildGetCurrentLobbyResponse(
                    true,
                    MatchMessageCode.CurrentLobbyRetrieved,
                    MatchMapper.ToMatchLobbyDto(currentMatch));
            }
        }
    }
}
