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
    internal class JoinLobbyUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public JoinLobbyUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<JoinLobbyResponse> ExecuteAsync(JoinLobbyRequest request)
        {
            MatchMessageCode? validationResult =
                MatchValidator.ValidateJoinLobby(request);

            if (validationResult.HasValue)
            {
                return MatchResponseFactory.BuildJoinLobbyResponse(
                    false,
                    validationResult.Value,
                    null);
            }

            string languageCode = request.GuestLanguageCode.Trim().ToLowerInvariant();

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult guestAvailability =
                    await PlayerAvailabilityPolicy.ValidateForMatchAsync(
                        unitOfWork,
                        request.GuestAccountId);

                if (!guestAvailability.IsAvailable)
                {
                    return MatchResponseFactory.BuildJoinLobbyResponse(
                        false,
                        guestAvailability.MessageCode,
                        null);
                }

                bool hasActiveMatch = await ActiveMatchPolicy.HasActiveMatchAsync(
                    unitOfWork,
                    guestAvailability.Player.PlayerId);

                if (hasActiveMatch)
                {
                    return MatchResponseFactory.BuildJoinLobbyResponse(
                        false,
                        MatchMessageCode.PlayerAlreadyInActiveMatch,
                        null);
                }

                MatchTransporter match =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                if (match == null)
                {
                    return MatchResponseFactory.BuildJoinLobbyResponse(
                        false,
                        MatchMessageCode.MatchNotFound,
                        null);
                }

                if (match.MatchStatus != MatchStatusConstants.WaitingForGuest ||
                    match.GuestId.HasValue)
                {
                    return MatchResponseFactory.BuildJoinLobbyResponse(
                        false,
                        MatchMessageCode.MatchNotAvailable,
                        null);
                }

                if (match.HostId == guestAvailability.Player.PlayerId)
                {
                    return MatchResponseFactory.BuildJoinLobbyResponse(
                        false,
                        MatchMessageCode.CannotJoinOwnMatch,
                        null);
                }

                DateTime votingStartedAt = DateTime.UtcNow;
                DateTime votingEndsAt = votingStartedAt.AddSeconds(
                    MatchTimingConstants.CategoryVotingDurationSeconds);

                bool joined = await unitOfWork.Matches.JoinAsync(
                    new JoinMatchTransporter
                    {
                        MatchId = request.MatchId,
                        GuestId = guestAvailability.Player.PlayerId,
                        GuestLanguageCode = languageCode,
                        MatchStatus = MatchStatusConstants.VotingCategory,
                        CategoryVotingStartedAt = votingStartedAt,
                        CategoryVotingEndsAt = votingEndsAt
                    });

                if (!joined)
                {
                    return MatchResponseFactory.BuildJoinLobbyResponse(
                        false,
                        MatchMessageCode.LobbyJoinFailed,
                        null);
                }

                await unitOfWork.CommitAsync();

                MatchTransporter updatedMatch =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                return MatchResponseFactory.BuildJoinLobbyResponse(
                    true,
                    MatchMessageCode.LobbyJoined,
                    MatchMapper.ToMatchLobbyDto(updatedMatch));
            }
        }
    }
}
