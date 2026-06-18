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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.MatchGuess
{
    internal class GuessLetterUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public GuessLetterUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<GuessLetterResponse> ExecuteAsync(
            GuessLetterRequest request)
        {
            if (request == null)
            {
                return MatchGuessResponseFactory.BuildGuessLetterResponse(
                    false,
                    MatchMessageCode.InvalidMatchId,
                    false,
                    false,
                    null);
            }

            MatchMessageCode? validationResult =
                MatchGuessValidator.ValidateGuessLetter(request);

            if (validationResult.HasValue)
            {
                return MatchGuessResponseFactory.BuildGuessLetterResponse(
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
                    return MatchGuessResponseFactory.BuildGuessLetterResponse(
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
                    return MatchGuessResponseFactory.BuildGuessLetterResponse(
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
                    return MatchGuessResponseFactory.BuildGuessLetterResponse(
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
                        return MatchGuessResponseFactory.BuildGuessLetterResponse(
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

                    return MatchGuessResponseFactory.BuildGuessLetterResponse(
                        false,
                        MatchMessageCode.GuessTurnExpired,
                        false,
                        true,
                        expiredState);
                }

                string normalizedLetter =
                    GuessTextNormalizer.Normalize(request.Letter);

                bool letterAlreadyGuessed =
                    await unitOfWork.MatchGuesses.LetterExistsAsync(
                        match.MatchId,
                        normalizedLetter);

                if (letterAlreadyGuessed)
                {
                    return MatchGuessResponseFactory.BuildGuessLetterResponse(
                        false,
                        MatchMessageCode.LetterAlreadyGuessed,
                        false,
                        false,
                        currentState);
                }

                string word = GuessEvaluator.GetWordForPlayer(match, playerId);
                bool isCorrect = GuessEvaluator.ContainsLetter(
                    word,
                    normalizedLetter);

                unitOfWork.MatchGuesses.Add(
                    new CreateMatchGuessTransporter
                    {
                        MatchId = match.MatchId,
                        GuessedById = playerId,
                        Letter = normalizedLetter,
                        IsCorrect = isCorrect
                    });

                MatchMessageCode messageCode;
                bool matchFinished = false;

                if (isCorrect)
                {
                    List<MatchGuessTransporter> previousGuesses =
                        await unitOfWork.MatchGuesses.GetByMatchIdAsync(
                            match.MatchId);

                    List<string> correctLetters = previousGuesses
                        .Where(guess => guess.IsCorrect)
                        .Select(guess => guess.Letter)
                        .ToList();

                    correctLetters.Add(normalizedLetter);

                    bool wordCompleted =
                        MatchGuessCompletionHelper.IsWordCompleted(
                            word,
                            correctLetters);

                    if (wordCompleted)
                    {
                        bool finished =
                            await MatchGuessCompletionHelper
                                .FinishWithGuesserWinAsync(unitOfWork, match);

                        if (!finished)
                        {
                            return MatchGuessResponseFactory
                                .BuildGuessLetterResponse(
                                    false,
                                    MatchMessageCode.UnexpectedError,
                                    isCorrect,
                                    false,
                                    currentState);
                        }

                        messageCode = MatchMessageCode.GuesserWon;
                        matchFinished = true;
                    }
                    else
                    {
                        bool turnUpdated =
                            await unitOfWork.Matches.UpdateGuessTurnAsync(
                                GuessTurnClock.CreateNextTurn(match.MatchId));

                        if (!turnUpdated)
                        {
                            return MatchGuessResponseFactory
                                .BuildGuessLetterResponse(
                                    false,
                                    MatchMessageCode.UnexpectedError,
                                    isCorrect,
                                    false,
                                    currentState);
                        }

                        messageCode = MatchMessageCode.CorrectLetterGuess;
                    }
                }
                else
                {
                    bool failedAttemptUpdated =
                        await unitOfWork.Matches.IncrementFailedAttemptsAsync(
                            match.MatchId);

                    if (!failedAttemptUpdated)
                    {
                        return MatchGuessResponseFactory.BuildGuessLetterResponse(
                            false,
                            MatchMessageCode.UnexpectedError,
                            isCorrect,
                            false,
                            currentState);
                    }

                    int updatedFailedAttempts = match.FailedAttempts + 1;

                    if (updatedFailedAttempts >= match.MaxAttempts)
                    {
                        bool finished =
                            await MatchGuessCompletionHelper
                                .FinishWithHostWinAsync(unitOfWork, match);

                        if (!finished)
                        {
                            return MatchGuessResponseFactory
                                .BuildGuessLetterResponse(
                                    false,
                                    MatchMessageCode.UnexpectedError,
                                    isCorrect,
                                    false,
                                    currentState);
                        }

                        messageCode = MatchMessageCode.HostWon;
                        matchFinished = true;
                    }
                    else
                    {
                        bool turnUpdated =
                            await unitOfWork.Matches.UpdateGuessTurnAsync(
                                GuessTurnClock.CreateNextTurn(match.MatchId));

                        if (!turnUpdated)
                        {
                            return MatchGuessResponseFactory
                                .BuildGuessLetterResponse(
                                    false,
                                    MatchMessageCode.UnexpectedError,
                                    isCorrect,
                                    false,
                                    currentState);
                        }

                        messageCode = MatchMessageCode.IncorrectLetterGuess;
                    }
                }

                await unitOfWork.CommitAsync();

                MatchGameStateDto updatedState =
                    await MatchGameStateLoader.BuildByMatchIdAsync(
                        unitOfWork,
                        match.MatchId,
                        playerId);

                return MatchGuessResponseFactory.BuildGuessLetterResponse(
                    true,
                    messageCode,
                    isCorrect,
                    matchFinished,
                    updatedState);
            }
        }
    }
}
