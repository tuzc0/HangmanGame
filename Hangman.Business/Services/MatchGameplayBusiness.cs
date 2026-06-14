using Hangman.Business.Constants;
using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Validators;
using Hangman.Contracts.Match;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Transporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class MatchGameplayBusiness : IMatchGameplayBusiness
    {
        private static readonly object RandomSyncRoot = new object();
        private static readonly Random RandomGenerator = new Random();

        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public MatchGameplayBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<VoteCategoryResponse> VoteCategoryAsync(
            VoteCategoryRequest request)
        {
            MatchMessageCode? validationResult =
                MatchGameplayValidator.ValidateVoteCategory(request);

            if (validationResult.HasValue)
            {
                return BuildVoteCategoryResponse(false, validationResult.Value, null);
            }

            using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await ValidatePlayerAvailabilityAsync(unitOfWork, request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return BuildVoteCategoryResponse(
                        false,
                        playerAvailability.MessageCode,
                        null);
                }

                MatchTransporter match = await unitOfWork.Matches.GetByIdAsync(
                    request.MatchId);

                MatchMessageCode? matchValidation =
                    ValidateMatchForPlayer(match, playerAvailability.Player.PlayerId);

                if (matchValidation.HasValue)
                {
                    return BuildVoteCategoryResponse(
                        false,
                        matchValidation.Value,
                        null);
                }

                if (match.MatchStatus != MatchStatusConstants.VotingCategory)
                {
                    CategoryVotingStateDto currentState =
                        await BuildCategoryVotingStateDtoAsync(
                            unitOfWork,
                            match,
                            playerAvailability.Player.PlayerId);

                    return BuildVoteCategoryResponse(
                        false,
                        MatchMessageCode.CategoryVotingNotActive,
                        currentState);
                }

                if (HasCategoryVotingExpired(match))
                {
                    MatchTransporter resolvedMatch =
                        await ResolveCategoryVotingInternalAsync(unitOfWork, match);

                    await unitOfWork.CommitAsync();

                    CategoryVotingStateDto resolvedState =
                        await BuildCategoryVotingStateDtoAsync(
                            unitOfWork,
                            resolvedMatch,
                            playerAvailability.Player.PlayerId);

                    return BuildVoteCategoryResponse(
                        false,
                        MatchMessageCode.CategoryVotingExpired,
                        resolvedState);
                }

                bool categoryExists =
                    await unitOfWork.Words.ExistsActiveCategoryAsync(request.CategoryId);

                if (!categoryExists)
                {
                    return BuildVoteCategoryResponse(
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
                    return BuildVoteCategoryResponse(
                        false,
                        MatchMessageCode.UnexpectedError,
                        null);
                }

                await unitOfWork.CommitAsync();

                MatchTransporter updatedMatch =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                CategoryVotingStateDto votingState =
                    await BuildCategoryVotingStateDtoAsync(
                        unitOfWork,
                        updatedMatch,
                        playerAvailability.Player.PlayerId);

                MatchMessageCode messageCode = existingVote == null
                    ? MatchMessageCode.CategoryVoteRegistered
                    : MatchMessageCode.CategoryVoteUpdated;

                return BuildVoteCategoryResponse(true, messageCode, votingState);
            }
        }

        public async Task<GetCategoryVotingStateResponse> GetCategoryVotingStateAsync(
            GetCategoryVotingStateRequest request)
        {
            MatchMessageCode? validationResult =
                MatchGameplayValidator.ValidateGetCategoryVotingState(request);

            if (validationResult.HasValue)
            {
                return BuildGetCategoryVotingStateResponse(
                    false,
                    validationResult.Value,
                    null);
            }

            using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await ValidatePlayerAvailabilityAsync(unitOfWork, request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return BuildGetCategoryVotingStateResponse(
                        false,
                        playerAvailability.MessageCode,
                        null);
                }

                MatchTransporter match = await unitOfWork.Matches.GetByIdAsync(
                    request.MatchId);

                MatchMessageCode? matchValidation =
                    ValidateMatchForPlayer(match, playerAvailability.Player.PlayerId);

                if (matchValidation.HasValue)
                {
                    return BuildGetCategoryVotingStateResponse(
                        false,
                        matchValidation.Value,
                        null);
                }

                MatchMessageCode messageCode = MatchMessageCode.CategoryVotingStateRetrieved;

                if (match.MatchStatus == MatchStatusConstants.VotingCategory &&
                    HasCategoryVotingExpired(match))
                {
                    match = await ResolveCategoryVotingInternalAsync(unitOfWork, match);

                    await unitOfWork.CommitAsync();

                    messageCode = MatchMessageCode.CategoryVotingResolved;
                }

                CategoryVotingStateDto votingState =
                    await BuildCategoryVotingStateDtoAsync(
                        unitOfWork,
                        match,
                        playerAvailability.Player.PlayerId);

                return BuildGetCategoryVotingStateResponse(
                    true,
                    messageCode,
                    votingState);
            }
        }

        public async Task<ResolveCategoryVotingResponse> ResolveCategoryVotingAsync(
            ResolveCategoryVotingRequest request)
        {
            MatchMessageCode? validationResult =
                MatchGameplayValidator.ValidateResolveCategoryVoting(request);

            if (validationResult.HasValue)
            {
                return BuildResolveCategoryVotingResponse(
                    false,
                    validationResult.Value,
                    null,
                    null);
            }

            using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await ValidatePlayerAvailabilityAsync(unitOfWork, request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return BuildResolveCategoryVotingResponse(
                        false,
                        playerAvailability.MessageCode,
                        null,
                        null);
                }

                MatchTransporter match = await unitOfWork.Matches.GetByIdAsync(
                    request.MatchId);

                MatchMessageCode? matchValidation =
                    ValidateMatchForPlayer(match, playerAvailability.Player.PlayerId);

                if (matchValidation.HasValue)
                {
                    return BuildResolveCategoryVotingResponse(
                        false,
                        matchValidation.Value,
                        null,
                        null);
                }

                if (match.MatchStatus != MatchStatusConstants.VotingCategory)
                {
                    CategoryVotingStateDto currentState =
                        await BuildCategoryVotingStateDtoAsync(
                            unitOfWork,
                            match,
                            playerAvailability.Player.PlayerId);

                    return BuildResolveCategoryVotingResponse(
                        true,
                        MatchMessageCode.CategoryVotingStateRetrieved,
                        BuildMatchLobbyDto(match),
                        currentState);
                }

                if (!HasCategoryVotingExpired(match))
                {
                    CategoryVotingStateDto currentState =
                        await BuildCategoryVotingStateDtoAsync(
                            unitOfWork,
                            match,
                            playerAvailability.Player.PlayerId);

                    return BuildResolveCategoryVotingResponse(
                        true,
                        MatchMessageCode.CategoryVotingStateRetrieved,
                        BuildMatchLobbyDto(match),
                        currentState);
                }

                MatchTransporter resolvedMatch =
                    await ResolveCategoryVotingInternalAsync(unitOfWork, match);

                if (resolvedMatch == null)
                {
                    return BuildResolveCategoryVotingResponse(
                        false,
                        MatchMessageCode.CategoryVotingResolveFailed,
                        null,
                        null);
                }

                await unitOfWork.CommitAsync();

                MatchTransporter updatedMatch =
                    await unitOfWork.Matches.GetByIdAsync(match.MatchId);

                CategoryVotingStateDto votingState =
                    await BuildCategoryVotingStateDtoAsync(
                        unitOfWork,
                        updatedMatch,
                        playerAvailability.Player.PlayerId);

                return BuildResolveCategoryVotingResponse(
                    true,
                    MatchMessageCode.CategoryVotingResolved,
                    BuildMatchLobbyDto(updatedMatch),
                    votingState);
            }
        }

        public async Task<GetSelectableWordsResponse> GetSelectableWordsAsync(
            GetSelectableWordsRequest request)
        {
            MatchMessageCode? validationResult =
                MatchGameplayValidator.ValidateGetSelectableWords(request);

            if (validationResult.HasValue)
            {
                return BuildGetSelectableWordsResponse(
                    false,
                    validationResult.Value,
                    new List<SelectableWordDto>());
            }

            using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await ValidatePlayerAvailabilityAsync(unitOfWork, request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return BuildGetSelectableWordsResponse(
                        false,
                        playerAvailability.MessageCode,
                        new List<SelectableWordDto>());
                }

                MatchTransporter match = await unitOfWork.Matches.GetByIdAsync(
                    request.MatchId);

                MatchMessageCode? matchValidation =
                    ValidateMatchForPlayer(match, playerAvailability.Player.PlayerId);

                if (matchValidation.HasValue)
                {
                    return BuildGetSelectableWordsResponse(
                        false,
                        matchValidation.Value,
                        new List<SelectableWordDto>());
                }

                if (match.HostId != playerAvailability.Player.PlayerId)
                {
                    return BuildGetSelectableWordsResponse(
                        false,
                        MatchMessageCode.PlayerNotHost,
                        new List<SelectableWordDto>());
                }

                if (match.MatchStatus != MatchStatusConstants.WaitingForHostWord ||
                    !match.SelectedCategoryId.HasValue)
                {
                    return BuildGetSelectableWordsResponse(
                        false,
                        MatchMessageCode.WordSelectionNotActive,
                        new List<SelectableWordDto>());
                }

                if (HasWordSelectionExpired(match))
                {
                    return BuildGetSelectableWordsResponse(
                        false,
                        MatchMessageCode.WordSelectionExpired,
                        new List<SelectableWordDto>());
                }

                List<WordTransporter> words =
                    await unitOfWork.Words.GetActiveByCategoryIdAndLanguageAsync(
                        match.SelectedCategoryId.Value,
                        match.HostLanguageCode);

                if (words == null || words.Count == 0)
                {
                    return BuildGetSelectableWordsResponse(
                        false,
                        MatchMessageCode.WordNotAvailable,
                        new List<SelectableWordDto>());
                }

                List<SelectableWordDto> wordDtos = words
                    .Select(BuildSelectableWordDto)
                    .Where(word => word != null)
                    .ToList();

                return BuildGetSelectableWordsResponse(
                    true,
                    MatchMessageCode.WordSelectionStateRetrieved,
                    wordDtos);
            }
        }

        public async Task<SelectWordResponse> SelectWordAsync(
            SelectWordRequest request)
        {
            MatchMessageCode? validationResult =
                MatchGameplayValidator.ValidateSelectWord(request);

            if (validationResult.HasValue)
            {
                return BuildSelectWordResponse(false, validationResult.Value, null);
            }

            using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await ValidatePlayerAvailabilityAsync(unitOfWork, request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return BuildSelectWordResponse(
                        false,
                        playerAvailability.MessageCode,
                        null);
                }

                MatchTransporter match = await unitOfWork.Matches.GetByIdAsync(
                    request.MatchId);

                MatchMessageCode? matchValidation =
                    ValidateMatchForPlayer(match, playerAvailability.Player.PlayerId);

                if (matchValidation.HasValue)
                {
                    return BuildSelectWordResponse(
                        false,
                        matchValidation.Value,
                        null);
                }

                if (match.HostId != playerAvailability.Player.PlayerId)
                {
                    return BuildSelectWordResponse(
                        false,
                        MatchMessageCode.PlayerNotHost,
                        null);
                }

                if (match.MatchStatus != MatchStatusConstants.WaitingForHostWord ||
                    !match.SelectedCategoryId.HasValue)
                {
                    return BuildSelectWordResponse(
                        false,
                        MatchMessageCode.WordSelectionNotActive,
                        null);
                }

                if (HasWordSelectionExpired(match))
                {
                    return BuildSelectWordResponse(
                        false,
                        MatchMessageCode.WordSelectionExpired,
                        null);
                }

                WordTransporter selectedWord =
                    await unitOfWork.Words.GetActiveByIdAsync(
                        request.WordId,
                        match.HostLanguageCode);

                if (selectedWord == null ||
                    selectedWord.CategoryId != match.SelectedCategoryId.Value)
                {
                    return BuildSelectWordResponse(
                        false,
                        MatchMessageCode.WordNotAvailable,
                        null);
                }

                bool selected = await unitOfWork.Matches.UpdateSelectedWordAsync(
                    new SelectMatchWordTransporter
                    {
                        MatchId = match.MatchId,
                        SelectedWordId = request.WordId,
                        MatchStatus = MatchStatusConstants.InProgress
                    });

                if (!selected)
                {
                    return BuildSelectWordResponse(
                        false,
                        MatchMessageCode.WordSelectionFailed,
                        null);
                }

                await unitOfWork.CommitAsync();

                MatchTransporter updatedMatch =
                    await unitOfWork.Matches.GetByIdAsync(match.MatchId);

                return BuildSelectWordResponse(
                    true,
                    MatchMessageCode.WordSelected,
                    BuildMatchLobbyDto(updatedMatch));
            }
        }

        private static async Task<MatchTransporter> ResolveCategoryVotingInternalAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match)
        {
            if (match == null ||
                match.MatchStatus != MatchStatusConstants.VotingCategory)
            {
                return match;
            }

            List<MatchCategoryVoteTransporter> votes =
                await unitOfWork.MatchCategoryVotes.GetByMatchIdAsync(match.MatchId);

            int selectedCategoryId =
                await ResolveSelectedCategoryIdAsync(unitOfWork, match, votes);

            if (selectedCategoryId <= 0)
            {
                return null;
            }

            DateTime currentDate = DateTime.UtcNow;

            bool updated = await unitOfWork.Matches.UpdateSelectedCategoryAsync(
                new SelectMatchCategoryTransporter
                {
                    MatchId = match.MatchId,
                    SelectedCategoryId = selectedCategoryId,
                    MatchStatus = MatchStatusConstants.WaitingForHostWord,
                    WordSelectionStartedAt = currentDate,
                    WordSelectionEndsAt = currentDate.AddSeconds(
                        MatchTimingConstants.HostWordSelectionDurationSeconds)
                });

            if (!updated)
            {
                return null;
            }

            match.SelectedCategoryId = selectedCategoryId;
            match.MatchStatus = MatchStatusConstants.WaitingForHostWord;
            match.WordSelectionStartedAt = currentDate;
            match.WordSelectionEndsAt = currentDate.AddSeconds(
                MatchTimingConstants.HostWordSelectionDurationSeconds);

            return match;
        }

        private static async Task<int> ResolveSelectedCategoryIdAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match,
            List<MatchCategoryVoteTransporter> votes)
        {
            if (votes != null && votes.Count > 0)
            {
                List<int> votedCategoryIds = votes
                    .Select(vote => vote.CategoryId)
                    .Distinct()
                    .ToList();

                if (votedCategoryIds.Count == 1)
                {
                    return votedCategoryIds[0];
                }

                return votedCategoryIds[GetRandomIndex(votedCategoryIds.Count)];
            }

            List<CategoryTransporter> categories =
                await unitOfWork.Words.GetActiveCategoriesByLanguageAsync(
                    match.HostLanguageCode);

            if (categories == null || categories.Count == 0)
            {
                return 0;
            }

            return categories[GetRandomIndex(categories.Count)].CategoryId;
        }

        private static int GetRandomIndex(int maxExclusive)
        {
            lock (RandomSyncRoot)
            {
                return RandomGenerator.Next(maxExclusive);
            }
        }

        private static bool HasCategoryVotingExpired(MatchTransporter match)
        {
            return match == null ||
                   !match.CategoryVotingEndsAt.HasValue ||
                   DateTime.UtcNow > match.CategoryVotingEndsAt.Value;
        }

        private static bool HasWordSelectionExpired(MatchTransporter match)
        {
            return match != null &&
                   match.WordSelectionEndsAt.HasValue &&
                   DateTime.UtcNow > match.WordSelectionEndsAt.Value;
        }

        private static MatchMessageCode? ValidateMatchForPlayer(
            MatchTransporter match,
            int playerId)
        {
            if (match == null)
            {
                return MatchMessageCode.MatchNotFound;
            }

            bool playerBelongsToMatch =
                match.HostId == playerId ||
                match.GuestId == playerId;

            if (!playerBelongsToMatch)
            {
                return MatchMessageCode.PlayerNotInMatch;
            }

            if (match.MatchStatus == MatchStatusConstants.Finished ||
                match.MatchStatus == MatchStatusConstants.Abandoned ||
                match.MatchStatus == MatchStatusConstants.Cancelled)
            {
                return MatchMessageCode.MatchAlreadyResolved;
            }

            return null;
        }

        private static async Task<PlayerAvailabilityResult> ValidatePlayerAvailabilityAsync(
            IUnitOfWork unitOfWork,
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
                return PlayerAvailabilityResult.Fail(
                    MatchMessageCode.EmailVerificationRequired);
            }

            if (account.AccountStatus != AccountStatusConstants.Active)
            {
                return PlayerAvailabilityResult.Fail(MatchMessageCode.AccountNotAvailable);
            }

            PlayerTransporter player = await unitOfWork.Players.GetByIdAsync(
                account.PlayerId);

            if (player == null || !player.IsActive)
            {
                return PlayerAvailabilityResult.Fail(
                    MatchMessageCode.PlayerProfileNotAvailable);
            }

            return PlayerAvailabilityResult.Success(player);
        }

        private static async Task<CategoryVotingStateDto> BuildCategoryVotingStateDtoAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match,
            int currentPlayerId)
        {
            if (match == null)
            {
                return null;
            }

            string languageCode = GetCurrentPlayerLanguageCode(match, currentPlayerId);

            List<MatchCategoryVoteTransporter> votes =
                await unitOfWork.MatchCategoryVotes.GetByMatchIdAsync(
                    match.MatchId,
                    languageCode);

            return new CategoryVotingStateDto
            {
                MatchId = match.MatchId,
                MatchStatus = match.MatchStatus,
                SelectedCategoryId = match.SelectedCategoryId,
                SelectedCategoryName = GetSelectedCategoryName(match, currentPlayerId),
                CategoryVotingStartedAt = match.CategoryVotingStartedAt,
                CategoryVotingEndsAt = match.CategoryVotingEndsAt,
                WordSelectionStartedAt = match.WordSelectionStartedAt,
                WordSelectionEndsAt = match.WordSelectionEndsAt,
                RemainingVotingSeconds = GetRemainingVotingSeconds(match),
                CanVote = CanVote(match),
                IsVotingResolved = IsVotingResolved(match),
                CanCurrentPlayerSelectWord = CanCurrentPlayerSelectWord(
                    match,
                    currentPlayerId),
                Votes = votes
                    .Select(BuildCategoryVoteDto)
                    .Where(vote => vote != null)
                    .ToList()
            };
        }

        private static string GetCurrentPlayerLanguageCode(
            MatchTransporter match,
            int currentPlayerId)
        {
            if (match.HostId == currentPlayerId)
            {
                return match.HostLanguageCode;
            }

            return match.GuestLanguageCode;
        }

        private static string GetSelectedCategoryName(
            MatchTransporter match,
            int currentPlayerId)
        {
            if (match.HostId == currentPlayerId)
            {
                return match.HostCategoryName;
            }

            return match.GuestCategoryName;
        }

        private static int GetRemainingVotingSeconds(MatchTransporter match)
        {
            if (match == null || !match.CategoryVotingEndsAt.HasValue)
            {
                return 0;
            }

            double remainingSeconds =
                (match.CategoryVotingEndsAt.Value - DateTime.UtcNow).TotalSeconds;

            if (remainingSeconds <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(remainingSeconds);
        }

        private static bool CanVote(MatchTransporter match)
        {
            return match != null &&
                   match.MatchStatus == MatchStatusConstants.VotingCategory &&
                   match.CategoryVotingEndsAt.HasValue &&
                   DateTime.UtcNow <= match.CategoryVotingEndsAt.Value;
        }

        private static bool IsVotingResolved(MatchTransporter match)
        {
            return match != null &&
                   match.SelectedCategoryId.HasValue &&
                   match.MatchStatus != MatchStatusConstants.VotingCategory;
        }

        private static bool CanCurrentPlayerSelectWord(
            MatchTransporter match,
            int currentPlayerId)
        {
            return match != null &&
                   match.MatchStatus == MatchStatusConstants.WaitingForHostWord &&
                   match.HostId == currentPlayerId;
        }

        private static CategoryVoteDto BuildCategoryVoteDto(
            MatchCategoryVoteTransporter vote)
        {
            if (vote == null)
            {
                return null;
            }

            return new CategoryVoteDto
            {
                PlayerId = vote.PlayerId,
                CategoryId = vote.CategoryId,
                CategoryName = vote.CategoryName,
                LanguageCode = vote.LanguageCode,
                CreatedAt = vote.CreatedAt,
                UpdatedAt = vote.UpdatedAt
            };
        }

        private static SelectableWordDto BuildSelectableWordDto(
            WordTransporter word)
        {
            if (word == null)
            {
                return null;
            }

            return new SelectableWordDto
            {
                WordId = word.WordId,
                CategoryId = word.CategoryId,
                CategoryName = word.CategoryName,
                WordText = word.WordText,
                Description = word.Description,
                LanguageCode = word.LanguageCode
            };
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
                MatchStatus = match.MatchStatus,
                CreatedAt = match.CreatedAt,
                JoinedAt = match.JoinedAt,
                CategoryVotingStartedAt = match.CategoryVotingStartedAt,
                CategoryVotingEndsAt = match.CategoryVotingEndsAt
            };
        }

        private static VoteCategoryResponse BuildVoteCategoryResponse(
            bool success,
            Enum messageCode,
            CategoryVotingStateDto votingState)
        {
            return new VoteCategoryResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                VotingState = votingState
            };
        }

        private static GetCategoryVotingStateResponse BuildGetCategoryVotingStateResponse(
            bool success,
            Enum messageCode,
            CategoryVotingStateDto votingState)
        {
            return new GetCategoryVotingStateResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                VotingState = votingState
            };
        }

        private static ResolveCategoryVotingResponse BuildResolveCategoryVotingResponse(
            bool success,
            Enum messageCode,
            MatchLobbyDto lobby,
            CategoryVotingStateDto votingState)
        {
            return new ResolveCategoryVotingResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobby = lobby,
                VotingState = votingState
            };
        }

        private static GetSelectableWordsResponse BuildGetSelectableWordsResponse(
            bool success,
            Enum messageCode,
            List<SelectableWordDto> words)
        {
            return new GetSelectableWordsResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Words = words
            };
        }

        private static SelectWordResponse BuildSelectWordResponse(
            bool success,
            Enum messageCode,
            MatchLobbyDto lobby)
        {
            return new SelectWordResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobby = lobby
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
                    MessageCode = MatchMessageCode.CurrentLobbyRetrieved,
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
