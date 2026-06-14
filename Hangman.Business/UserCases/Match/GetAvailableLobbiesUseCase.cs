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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.Match
{
    internal class GetAvailableLobbiesUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public GetAvailableLobbiesUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<GetAvailableLobbiesResponse> ExecuteAsync(
            GetAvailableLobbiesRequest request)
        {
            MatchMessageCode? validationResult =
                MatchValidator.ValidateGetAvailableLobbies(request);

            if (validationResult.HasValue)
            {
                return MatchResponseFactory.BuildGetAvailableLobbiesResponse(
                    false,
                    validationResult.Value,
                    new List<AvailableLobbyDto>());
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await PlayerAvailabilityPolicy.ValidateForMatchAsync(
                        unitOfWork,
                        request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return MatchResponseFactory.BuildGetAvailableLobbiesResponse(
                        false,
                        playerAvailability.MessageCode,
                        new List<AvailableLobbyDto>());
                }

                List<AvailableMatchTransporter> availableMatches =
                    await unitOfWork.Matches.GetAvailableByStatusAsync(
                        MatchStatusConstants.WaitingForGuest);

                List<AvailableLobbyDto> lobbies = availableMatches
                    .Where(match => match.HostId != playerAvailability.Player.PlayerId)
                    .Select(MatchMapper.ToAvailableLobbyDto)
                    .Where(lobby => lobby != null)
                    .ToList();

                return MatchResponseFactory.BuildGetAvailableLobbiesResponse(
                    true,
                    MatchMessageCode.AvailableLobbiesRetrieved,
                    lobbies);
            }
        }
    }
}
