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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.MatchGameplay.WordSelection
{
    internal class GetSelectableWordsUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public GetSelectableWordsUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<GetSelectableWordsResponse> ExecuteAsync(
            GetSelectableWordsRequest request)
        {
            MatchMessageCode? validationResult =
                MatchGameplayValidator.ValidateGetSelectableWords(request);

            if (validationResult.HasValue)
            {
                return MatchGameplayResponseFactory.BuildGetSelectableWordsResponse(
                    false,
                    validationResult.Value,
                    new List<SelectableWordDto>());
            }

            using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult playerAvailability =
                    await PlayerAvailabilityPolicy.ValidateForMatchAsync(
                        unitOfWork,
                        request.AccountId);

                if (!playerAvailability.IsAvailable)
                {
                    return MatchGameplayResponseFactory.BuildGetSelectableWordsResponse(
                        false,
                        playerAvailability.MessageCode,
                        new List<SelectableWordDto>());
                }

                MatchTransporter match =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                MatchMessageCode? matchValidation =
                    MatchAccessPolicy.ValidateMatchForPlayer(
                        match,
                        playerAvailability.Player.PlayerId);

                if (matchValidation.HasValue)
                {
                    return MatchGameplayResponseFactory.BuildGetSelectableWordsResponse(
                        false,
                        matchValidation.Value,
                        new List<SelectableWordDto>());
                }

                if (match.HostId != playerAvailability.Player.PlayerId)
                {
                    return MatchGameplayResponseFactory.BuildGetSelectableWordsResponse(
                        false,
                        MatchMessageCode.PlayerNotHost,
                        new List<SelectableWordDto>());
                }

                if (!WordSelectionPolicy.IsWordSelectionActive(match))
                {
                    return MatchGameplayResponseFactory.BuildGetSelectableWordsResponse(
                        false,
                        MatchMessageCode.WordSelectionNotActive,
                        new List<SelectableWordDto>());
                }

                if (WordSelectionPolicy.HasWordSelectionExpired(match))
                {
                    return MatchGameplayResponseFactory.BuildGetSelectableWordsResponse(
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
                    return MatchGameplayResponseFactory.BuildGetSelectableWordsResponse(
                        false,
                        MatchMessageCode.WordNotAvailable,
                        new List<SelectableWordDto>());
                }

                List<SelectableWordDto> wordDtos = words
                    .Select(SelectableWordMapper.ToSelectableWordDto)
                    .Where(word => word != null)
                    .ToList();

                return MatchGameplayResponseFactory.BuildGetSelectableWordsResponse(
                    true,
                    MatchMessageCode.WordSelectionStateRetrieved,
                    wordDtos);
            }
        }
    }
}
