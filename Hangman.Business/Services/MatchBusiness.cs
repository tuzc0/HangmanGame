using Hangman.Business.Constants;
using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Validators;
using Hangman.Contracts.Match;
using Hangman.DataAccess.Transporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class MatchBusiness : IMatchBusiness
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public MatchBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<CreateLobbyResponse> CreateLobbyAsync(CreateLobbyRequest request)
        {
            MatchMessageCode? validationResult = MatchValidator.ValidateCreateLobby(request);

            if (validationResult.HasValue)
            {
                return BuildCreateLobbyResponse(false, validationResult.Value, null);
            }

            string languageCode = request.HostLanguageCode.Trim().ToLowerInvariant();

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult hostAvailability =
                    await ValidatePlayerAvailabilityAsync(unitOfWork, request.HostAccountId);

                if (!hostAvailability.IsAvailable)
                {
                    return BuildCreateLobbyResponse(false, hostAvailability.MessageCode, null);
                }

                bool hasActiveMatch = await HasActiveMatchAsync(
                    unitOfWork,
                    hostAvailability.Player.PlayerId);

                if (hasActiveMatch)
                {
                    return BuildCreateLobbyResponse(
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

                MatchTransporter createdMatch = await GetLatestCreatedLobbyAsync(
                    unitOfWork,
                    hostAvailability.Player.PlayerId);

                if (createdMatch == null)
                {
                    return BuildCreateLobbyResponse(
                        false,
                        MatchMessageCode.LobbyCreationFailed,
                        null);
                }

                return BuildCreateLobbyResponse(
                    true,
                    MatchMessageCode.LobbyCreated,
                    BuildMatchLobbyDto(createdMatch));
            }
        }

        public async Task<GetAvailableLobbiesResponse> GetAvailableLobbiesAsync(
            GetAvailableLobbiesRequest request)
        {
            MatchMessageCode? validationResult =
                MatchValidator.ValidateGetAvailableLobbies(request);

            if (validationResult.HasValue)
            {
                return BuildGetAvailableLobbiesResponse(
                    false,
                    validationResult.Value,
                    new List<AvailableLobbyDto>());
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await ValidatePlayerAvailabilityAsync(unitOfWork, request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return BuildGetAvailableLobbiesResponse(
                        false,
                        playerAvailability.MessageCode,
                        new List<AvailableLobbyDto>());
                }

                List<AvailableMatchTransporter> availableMatches =
                    await unitOfWork.Matches.GetAvailableByStatusAsync(
                        MatchStatusConstants.WaitingForGuest);

                List<AvailableLobbyDto> lobbies = availableMatches
                    .Where(match => match.HostId != playerAvailability.Player.PlayerId)
                    .Select(BuildAvailableLobbyDto)
                    .ToList();

                return BuildGetAvailableLobbiesResponse(
                    true,
                    MatchMessageCode.AvailableLobbiesRetrieved,
                    lobbies);
            }
        }

        public async Task<JoinLobbyResponse> JoinLobbyAsync(JoinLobbyRequest request)
        {
            MatchMessageCode? validationResult = MatchValidator.ValidateJoinLobby(request);

            if (validationResult.HasValue)
            {
                return BuildJoinLobbyResponse(false, validationResult.Value, null);
            }

            string languageCode = request.GuestLanguageCode.Trim().ToLowerInvariant();

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult guestAvailability =
                    await ValidatePlayerAvailabilityAsync(unitOfWork, request.GuestAccountId);

                if (!guestAvailability.IsAvailable)
                {
                    return BuildJoinLobbyResponse(false, guestAvailability.MessageCode, null);
                }

                bool hasActiveMatch = await HasActiveMatchAsync(
                    unitOfWork,
                    guestAvailability.Player.PlayerId);

                if (hasActiveMatch)
                {
                    return BuildJoinLobbyResponse(
                        false,
                        MatchMessageCode.PlayerAlreadyInActiveMatch,
                        null);
                }

                MatchTransporter match = await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                if (match == null)
                {
                    return BuildJoinLobbyResponse(false, MatchMessageCode.MatchNotFound, null);
                }

                if (match.MatchStatus != MatchStatusConstants.WaitingForGuest ||
                    match.GuestId.HasValue)
                {
                    return BuildJoinLobbyResponse(false, MatchMessageCode.MatchNotAvailable, null);
                }

                if (match.HostId == guestAvailability.Player.PlayerId)
                {
                    return BuildJoinLobbyResponse(false, MatchMessageCode.CannotJoinOwnMatch, null);
                }

                DateTime votingStartedAt = DateTime.UtcNow;
                DateTime votingEndsAt = votingStartedAt.AddSeconds(
                    MatchTimingConstants.CategoryVotingDurationSeconds);

                bool joined = await unitOfWork.Matches.JoinAsync(new JoinMatchTransporter
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
                    return BuildJoinLobbyResponse(false, MatchMessageCode.LobbyJoinFailed, null);
                }

                await unitOfWork.CommitAsync();

                MatchTransporter updatedMatch = await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                return BuildJoinLobbyResponse(
                    true,
                    MatchMessageCode.LobbyJoined,
                    BuildMatchLobbyDto(updatedMatch));
            }
        }

        private static async Task<PlayerAvailabilityResult> ValidatePlayerAvailabilityAsync(
            DataAccess.Interfaces.IUnitOfWork unitOfWork,
            int accountId)
        {
            AccountTransporter account = await unitOfWork.Accounts.GetByIdAsync(accountId);

            if (account == null)
            {
                return PlayerAvailabilityResult.Fail(MatchMessageCode.AccountNotFound);
            }

            if (account.AccountStatus == AccountStatusConstants.Blocked ||
                account.AccountStatus == AccountStatusConstants.Deleted)
            {
                return PlayerAvailabilityResult.Fail(MatchMessageCode.AccountNotAvailable);
            }

            if (!account.IsEmailVerified ||
                account.AccountStatus == AccountStatusConstants.PendingVerification)
            {
                return PlayerAvailabilityResult.Fail(MatchMessageCode.EmailVerificationRequired);
            }

            if (account.AccountStatus != AccountStatusConstants.Active)
            {
                return PlayerAvailabilityResult.Fail(MatchMessageCode.AccountNotAvailable);
            }

            PlayerTransporter player = await unitOfWork.Players.GetByIdAsync(account.PlayerId);

            if (player == null || !player.IsActive)
            {
                return PlayerAvailabilityResult.Fail(
                    MatchMessageCode.PlayerProfileNotAvailable);
            }

            return PlayerAvailabilityResult.Success(player);
        }

        private static async Task<bool> HasActiveMatchAsync(
            DataAccess.Interfaces.IUnitOfWork unitOfWork,
            int playerId)
        {
            List<MatchTransporter> matches = await unitOfWork.Matches.GetByPlayerIdAsync(playerId);

            return matches.Any(match =>
                match.MatchStatus == MatchStatusConstants.WaitingForGuest ||
                match.MatchStatus == MatchStatusConstants.VotingCategory ||
                match.MatchStatus == MatchStatusConstants.WaitingForHostWord ||
                match.MatchStatus == MatchStatusConstants.InProgress);
        }

        private static async Task<MatchTransporter> GetLatestCreatedLobbyAsync(
            DataAccess.Interfaces.IUnitOfWork unitOfWork,
            int hostId)
        {
            List<MatchTransporter> matches = await unitOfWork.Matches.GetByPlayerIdAsync(hostId);

            return matches
                .Where(match =>
                    match.HostId == hostId &&
                    match.MatchStatus == MatchStatusConstants.WaitingForGuest)
                .OrderByDescending(match => match.CreatedAt)
                .FirstOrDefault();
        }

        public async Task<GetCurrentLobbyResponse> GetCurrentLobbyAsync(
    GetCurrentLobbyRequest request)
        {
            MatchMessageCode? validationResult =
                MatchValidator.ValidateGetCurrentLobby(request);

            if (validationResult.HasValue)
            {
                return BuildGetCurrentLobbyResponse(false, validationResult.Value, null);
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await ValidatePlayerAvailabilityAsync(unitOfWork, request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return BuildGetCurrentLobbyResponse(
                        false,
                        playerAvailability.MessageCode,
                        null);
                }

                MatchTransporter currentMatch = await GetCurrentActiveMatchAsync(
                    unitOfWork,
                    playerAvailability.Player.PlayerId);

                if (currentMatch == null)
                {
                    return BuildGetCurrentLobbyResponse(
                        true,
                        MatchMessageCode.NoActiveLobby,
                        null);
                }

                return BuildGetCurrentLobbyResponse(
                    true,
                    MatchMessageCode.CurrentLobbyRetrieved,
                    BuildMatchLobbyDto(currentMatch));
            }
        }

        public async Task<LeaveLobbyResponse> LeaveLobbyAsync(LeaveLobbyRequest request)
        {
            MatchMessageCode? validationResult = MatchValidator.ValidateLeaveLobby(request);

            if (validationResult.HasValue)
            {
                return BuildLeaveLobbyResponse(false, validationResult.Value);
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await ValidatePlayerAvailabilityAsync(unitOfWork, request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return BuildLeaveLobbyResponse(false, playerAvailability.MessageCode);
                }

                MatchTransporter match = await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                if (match == null)
                {
                    return BuildLeaveLobbyResponse(false, MatchMessageCode.MatchNotFound);
                }

                bool playerBelongsToMatch =
                    match.HostId == playerAvailability.Player.PlayerId ||
                    match.GuestId == playerAvailability.Player.PlayerId;

                if (!playerBelongsToMatch)
                {
                    return BuildLeaveLobbyResponse(false, MatchMessageCode.LobbyLeaveNotAllowed);
                }

                if (!IsActiveMatch(match))
                {
                    return BuildLeaveLobbyResponse(false, MatchMessageCode.MatchAlreadyResolved);
                }

                if (CanLeaveWithoutPenalty(match))
                {
                    bool finished = await FinishMatchWithoutPenaltyAsync(unitOfWork, match.MatchId);

                    if (!finished)
                    {
                        return BuildLeaveLobbyResponse(false, MatchMessageCode.LobbyLeaveFailed);
                    }

                    await unitOfWork.CommitAsync();

                    return BuildLeaveLobbyResponse(true, MatchMessageCode.LobbyLeft);
                }

                int penalizedPlayerId = playerAvailability.Player.PlayerId;
                int winnerPlayerId = GetWinnerPlayerId(match, penalizedPlayerId);

                if (winnerPlayerId <= 0)
                {
                    return BuildLeaveLobbyResponse(false, MatchMessageCode.LobbyLeaveFailed);
                }

                bool abandoned = await RegisterPenalizedAbandonAsync(
                    unitOfWork,
                    match.MatchId,
                    penalizedPlayerId,
                    winnerPlayerId);

                if (!abandoned)
                {
                    return BuildLeaveLobbyResponse(false, MatchMessageCode.LobbyLeaveFailed);
                }

                await unitOfWork.CommitAsync();

                return BuildLeaveLobbyResponse(true, MatchMessageCode.LobbyAbandoned);
            }
        }

        private static MatchLobbyDto BuildMatchLobbyDto(MatchTransporter match)
        {
            if (match == null)
            {
                return null;
            }

            return new MatchLobbyDto
            {
                MatchId = match.MatchId,
                HostId = match.HostId,
                HostFullName = match.HostFullName,
                HostLanguageCode = match.HostLanguageCode,
                GuestId = match.GuestId,
                GuestFullName = match.GuestFullName,
                GuestLanguageCode = match.GuestLanguageCode,

                SelectedCategoryId = match.SelectedCategoryId,
                SelectedCategoryName = match.HostCategoryName,
                SelectedWordId = match.SelectedWordId,
                WordSelectionStartedAt = match.WordSelectionStartedAt,
                WordSelectionEndsAt = match.WordSelectionEndsAt,
                StartedAt = match.StartedAt,
                FinishedAt = match.FinishedAt,
                WinnerId = match.WinnerId,
                PenalizedUserId = match.PenalizedUserId,

                MatchStatus = match.MatchStatus,
                CreatedAt = match.CreatedAt,
                JoinedAt = match.JoinedAt,
                CategoryVotingStartedAt = match.CategoryVotingStartedAt,
                CategoryVotingEndsAt = match.CategoryVotingEndsAt
            };
        }

        private static AvailableLobbyDto BuildAvailableLobbyDto(
            AvailableMatchTransporter match)
        {
            return new AvailableLobbyDto
            {
                MatchId = match.MatchId,
                HostId = match.HostId,
                HostFullName = match.HostFullName,
                HostEmail = match.HostEmail,
                HostLanguageCode = match.HostLanguageCode,
                MatchStatus = match.MatchStatus,
                CreatedAt = match.CreatedAt
            };
        }

        private static CreateLobbyResponse BuildCreateLobbyResponse(
            bool success,
            Enum messageCode,
            MatchLobbyDto lobby)
        {
            return new CreateLobbyResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobby = lobby
            };
        }

        private static GetAvailableLobbiesResponse BuildGetAvailableLobbiesResponse(
            bool success,
            Enum messageCode,
            List<AvailableLobbyDto> lobbies)
        {
            return new GetAvailableLobbiesResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobbies = lobbies
            };
        }

        private static JoinLobbyResponse BuildJoinLobbyResponse(
            bool success,
            Enum messageCode,
            MatchLobbyDto lobby)
        {
            return new JoinLobbyResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobby = lobby
            };
        }

        private static async Task<MatchTransporter> GetCurrentActiveMatchAsync(
            DataAccess.Interfaces.IUnitOfWork unitOfWork,
            int playerId)
        {
            List<MatchTransporter> matches = await unitOfWork.Matches.GetByPlayerIdAsync(playerId);

            return matches
                .Where(IsActiveMatch)
                .OrderByDescending(match => match.CreatedAt)
                .FirstOrDefault();
        }

        private static bool IsActiveMatch(MatchTransporter match)
        {
            if (match == null)
            {
                return false;
            }

            return match.MatchStatus == MatchStatusConstants.WaitingForGuest ||
                   match.MatchStatus == MatchStatusConstants.VotingCategory ||
                   match.MatchStatus == MatchStatusConstants.WaitingForHostWord ||
                   match.MatchStatus == MatchStatusConstants.InProgress;
        }

        private static bool CanLeaveWithoutPenalty(MatchTransporter match)
        {
            if (match == null)
            {
                return false;
            }

            if (match.MatchStatus == MatchStatusConstants.WaitingForGuest)
            {
                return true;
            }

            if (match.MatchStatus == MatchStatusConstants.VotingCategory)
            {
                DateTime safeLeaveDeadline = GetSafeLeaveDeadline(match);

                return DateTime.UtcNow <= safeLeaveDeadline;
            }

            return false;
        }

        private static DateTime GetSafeLeaveDeadline(MatchTransporter match)
        {
            DateTime baseDate = match.CategoryVotingStartedAt
                ?? match.JoinedAt
                ?? match.CreatedAt;

            return baseDate.AddSeconds(MatchTimingConstants.SafeLeaveDurationSeconds);
        }

        private static int GetWinnerPlayerId(
            MatchTransporter match,
            int penalizedPlayerId)
        {
            if (match == null)
            {
                return 0;
            }

            if (match.HostId == penalizedPlayerId)
            {
                return match.GuestId.HasValue ? match.GuestId.Value : 0;
            }

            if (match.GuestId.HasValue &&
                match.GuestId.Value == penalizedPlayerId)
            {
                return match.HostId;
            }

            return 0;
        }

        private static async Task<bool> FinishMatchWithoutPenaltyAsync(
            DataAccess.Interfaces.IUnitOfWork unitOfWork,
            int matchId)
        {
            return await unitOfWork.Matches.FinishAsync(
                new FinishMatchTransporter
                {
                    MatchId = matchId,
                    WinnerId = null,
                    MatchStatus = MatchStatusConstants.Finished
                });
        }

        private static async Task<bool> RegisterPenalizedAbandonAsync(
            DataAccess.Interfaces.IUnitOfWork unitOfWork,
            int matchId,
            int penalizedPlayerId,
            int winnerPlayerId)
        {
            bool abandoned = await unitOfWork.Matches.RegisterAbandonAsync(
                new AbandonMatchTransporter
                {
                    MatchId = matchId,
                    PenalizedUserId = penalizedPlayerId,
                    WinnerId = winnerPlayerId,
                    MatchStatus = MatchStatusConstants.Abandoned
                });

            if (!abandoned)
            {
                return false;
            }

            unitOfWork.ScoreMovements.Add(
                new CreateScoreMovementTransporter
                {
                    PlayerId = penalizedPlayerId,
                    MatchId = matchId,
                    Points = ScorePointsConstants.AbandonPenalty,
                    MovementType = ScoreMovementTypeConstants.AbandonPenalty
                });

            return true;
        }

        private static GetCurrentLobbyResponse BuildGetCurrentLobbyResponse(
            bool success,
            Enum messageCode,
            MatchLobbyDto lobby)
        {
            return new GetCurrentLobbyResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobby = lobby
            };
        }

        private static LeaveLobbyResponse BuildLeaveLobbyResponse(
            bool success,
            Enum messageCode)
        {
            return new LeaveLobbyResponse
            {
                Success = success,
                MessageCode = messageCode.ToString()
            };
        }

        private sealed class PlayerAvailabilityResult
        {
            public bool IsAvailable { get; private set; }

            public MatchMessageCode MessageCode { get; private set; }

            public PlayerTransporter Player { get; private set; }

            public static PlayerAvailabilityResult Success(PlayerTransporter player)
            {
                return new PlayerAvailabilityResult
                {
                    IsAvailable = true,
                    MessageCode = MatchMessageCode.LobbyCreated,
                    Player = player
                };
            }

            public static PlayerAvailabilityResult Fail(MatchMessageCode messageCode)
            {
                return new PlayerAvailabilityResult
                {
                    IsAvailable = false,
                    MessageCode = messageCode,
                    Player = null
                };
            }
        }
    }
}
