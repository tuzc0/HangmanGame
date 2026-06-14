using Hangman.Business.Constants;
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
    internal class CreateLobbyUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public CreateLobbyUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<CreateLobbyResponse> ExecuteAsync(CreateLobbyRequest request)
        {
            MatchMessageCode? validationResult =
                MatchValidator.ValidateCreateLobby(request);

            if (validationResult.HasValue)
            {
                return MatchResponseFactory.BuildCreateLobbyResponse(
                    false,
                    validationResult.Value,
                    null);
            }

            string languageCode = request.HostLanguageCode.Trim().ToLowerInvariant();

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult hostAvailability =
                    await PlayerAvailabilityPolicy.ValidateForMatchAsync(
                        unitOfWork,
                        request.HostAccountId);

                if (!hostAvailability.IsAvailable)
                {
                    return MatchResponseFactory.BuildCreateLobbyResponse(
                        false,
                        hostAvailability.MessageCode,
                        null);
                }

                bool hasActiveMatch = await ActiveMatchPolicy.HasActiveMatchAsync(
                    unitOfWork,
                    hostAvailability.Player.PlayerId);

                if (hasActiveMatch)
                {
                    return MatchResponseFactory.BuildCreateLobbyResponse(
                        false,
                        MatchMessageCode.PlayerAlreadyInActiveMatch,
                        null);
                }

                unitOfWork.Matches.Add(new CreateMatchTransporter
                {
                    HostId = hostAvailability.Player.PlayerId,
                    HostLanguageCode = languageCode,
                    MatchStatus = MatchStatusConstants.WaitingForGuest,
                    FailedAttempts = MatchDefaultsConstants.InitialFailedAttempts,
                    MaxAttempts = MatchDefaultsConstants.MaxAttempts
                });

                await unitOfWork.CommitAsync();

                MatchTransporter createdMatch =
                    await ActiveMatchPolicy.GetLatestCreatedLobbyAsync(
                        unitOfWork,
                        hostAvailability.Player.PlayerId);

                if (createdMatch == null)
                {
                    return MatchResponseFactory.BuildCreateLobbyResponse(
                        false,
                        MatchMessageCode.LobbyCreationFailed,
                        null);
                }

                return MatchResponseFactory.BuildCreateLobbyResponse(
                    true,
                    MatchMessageCode.LobbyCreated,
                    MatchMapper.ToMatchLobbyDto(createdMatch));
            }
        }
    }
}
