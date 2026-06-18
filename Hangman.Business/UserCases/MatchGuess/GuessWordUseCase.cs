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
    internal class GuessWordUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public GuessWordUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<GuessWordResponse> ExecuteAsync(
            GuessWordRequest request)
        {
            if (request == null)
            {
                return MatchGuessResponseFactory.BuildGuessWordResponse(
                    false,
                    MatchMessageCode.InvalidMatchId,
                    false,
                    false,
                    null);
            }

            MatchMessageCode? validationResult =
                MatchGuessValidator.ValidateGuessWord(request);

            if (validationResult.HasValue)
            {
                return MatchGuessResponseFactory.BuildGuessWordResponse(
                    false,
                    validationResult.Value,
                    false,
                    false,
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
                    return MatchGuessResponseFactory.BuildGuessWordResponse(
                        false,
                        playerAvailability.MessageCode,
                        false,
                        false,
                        null);
                }

                int playerId = playerAvailability.Player.PlayerId;

                MatchTransporter match =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                MatchMessageCode? accessValidation =
                    GuessAccessPolicy.ValidateGuestGuessAccess(match, playerId);

                if (accessValidation.HasValue)
                {
                    return MatchGuessResponseFactory.BuildGuessWordResponse(
                        false,
                        accessValidation.Value,
                        false,
                        false,
                        null);
                }

                MatchGameStateDto currentState =
                    await MatchGameStateLoader.BuildAsync(
                        unitOfWork,
                        match,
                        playerId);

                if (!GuessTurnPolicy.HasGuessTurnStarted(match))
                {
                    return MatchGuessResponseFactory.BuildGuessWordResponse(
                        false,
                        MatchMessageCode.GuessTurnNotStarted,
                        false,
                        false,
                        currentState);
                }

                if (GuessTurnPolicy.HasGuessTurnExpired(match))
                {
                    bool timeoutResolved =
                        await MatchGuessCompletionHelper.FinishWithHostWinAsync(
                            unitOfWork,
                            match);

                    if (!timeoutResolved)
                    {
                        return MatchGuessResponseFactory.BuildGuessWordResponse(
                            false,
                            MatchMessageCode.UnexpectedError,
                            false,
                            false,
                            currentState);
                    }

                    await unitOfWork.CommitAsync();

                    MatchGameStateDto expiredState =
                        await MatchGameStateLoader.BuildByMatchIdAsync(
                            unitOfWork,
                            match.MatchId,
                            playerId);

                    return MatchGuessResponseFactory.BuildGuessWordResponse(
                        false,
                        MatchMessageCode.GuessTurnExpired,
                        false,
                        true,
                        expiredState);
                }

                string word = GuessEvaluator.GetWordForPlayer(match, playerId);
                bool isCorrect = GuessEvaluator.WordMatches(
                    word,
                    request.Word);

                unitOfWork.MatchWordGuesses.Add(
                    new CreateMatchWordGuessTransporter
                    {
                        MatchId = match.MatchId,
                        GuessedById = playerId,
                        GuessedWord = request.Word.Trim(),
                        IsCorrect = isCorrect
                    });

                bool finished = isCorrect
                    ? await MatchGuessCompletionHelper.FinishWithGuesserWinAsync(
                        unitOfWork,
                        match)
                    : await MatchGuessCompletionHelper.FinishWithHostWinAsync(
                        unitOfWork,
                        match);

                if (!finished)
                {
                    return MatchGuessResponseFactory.BuildGuessWordResponse(
                        false,
                        MatchMessageCode.UnexpectedError,
                        isCorrect,
                        false,
                        currentState);
                }

                await unitOfWork.CommitAsync();

                MatchGameStateDto updatedState =
                    await MatchGameStateLoader.BuildByMatchIdAsync(
                        unitOfWork,
                        match.MatchId,
                        playerId);

                MatchMessageCode messageCode = isCorrect
                    ? MatchMessageCode.WordGuessCorrect
                    : MatchMessageCode.WordGuessIncorrect;

                return MatchGuessResponseFactory.BuildGuessWordResponse(
                    true,
                    messageCode,
                    isCorrect,
                    true,
                    updatedState);
            }
        }
    }
}
