using Hangman.Business.Constants;
using Hangman.Business.Factories;
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

namespace Hangman.Business.UserCases.MatchGameplay.WordSelection
{
    internal class SelectWordUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public SelectWordUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<SelectWordResponse> ExecuteAsync(
            SelectWordRequest request)
        {
            MatchMessageCode? validationResult =
                MatchGameplayValidator.ValidateSelectWord(request);

            if (validationResult.HasValue)
            {
                return MatchGameplayResponseFactory.BuildSelectWordResponse(
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
                    return MatchGameplayResponseFactory.BuildSelectWordResponse(
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
                    return MatchGameplayResponseFactory.BuildSelectWordResponse(
                        false,
                        matchValidation.Value,
                        null);
                }

                if (match.HostId != playerAvailability.Player.PlayerId)
                {
                    return MatchGameplayResponseFactory.BuildSelectWordResponse(
                        false,
                        MatchMessageCode.PlayerNotHost,
                        null);
                }

                if (!WordSelectionPolicy.IsWordSelectionActive(match))
                {
                    return MatchGameplayResponseFactory.BuildSelectWordResponse(
                        false,
                        MatchMessageCode.WordSelectionNotActive,
                        null);
                }

                if (WordSelectionPolicy.HasWordSelectionExpired(match))
                {
                    return MatchGameplayResponseFactory.BuildSelectWordResponse(
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
                    return MatchGameplayResponseFactory.BuildSelectWordResponse(
                        false,
                        MatchMessageCode.WordNotAvailable,
                        null);
                }

                DateTime currentDate = DateTime.UtcNow;

                bool selected = await unitOfWork.Matches.UpdateSelectedWordAsync(
                    new SelectMatchWordTransporter
                    {
                        MatchId = match.MatchId,
                        SelectedWordId = request.WordId,
                        StartedAt = currentDate,
                        GuessTurnStartedAt = currentDate,
                        GuessTurnEndsAt = currentDate.AddSeconds(
                            GuessConstants.GuessTurnDurationSeconds),
                        MatchStatus = MatchStatusConstants.InProgress
                    });

                if (!selected)
                {
                    return MatchGameplayResponseFactory.BuildSelectWordResponse(
                        false,
                        MatchMessageCode.WordSelectionFailed,
                        null);
                }

                await unitOfWork.CommitAsync();

                MatchTransporter updatedMatch =
                    await unitOfWork.Matches.GetByIdAsync(match.MatchId);

                return MatchGameplayResponseFactory.BuildSelectWordResponse(
                    true,
                    MatchMessageCode.WordSelected,
                    MatchMapper.ToMatchLobbyDto(updatedMatch));
            }
        }
    }
}
