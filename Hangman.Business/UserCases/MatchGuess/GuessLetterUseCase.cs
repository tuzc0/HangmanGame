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
                return BuildFailureResponse(
                    MatchMessageCode.InvalidMatchId,
                    null);
            }

            MatchMessageCode? validationResult =
                MatchGuessValidator.ValidateGuessLetter(request);

            if (validationResult.HasValue)
            {
                return BuildFailureResponse(
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
                    return BuildFailureResponse(
                        ToMatchMessageCode(playerAvailability.MessageCode),
                        null);
                }

                int playerId = playerAvailability.Player.PlayerId;

                MatchTransporter match =
                    await unitOfWork.Matches.GetByIdAsync(request.MatchId);

                MatchMessageCode? accessValidation =
                    GuessAccessPolicy.ValidateGuestGuessAccess(match, playerId);

                if (accessValidation.HasValue)
                {
                    return BuildFailureResponse(
                        accessValidation.Value,
                        null);
                }

                MatchGameStateDto currentState =
                    await MatchGameStateLoader.BuildAsync(
                        unitOfWork,
                        match,
                        playerId);

                if (!GuessTurnPolicy.HasGuessTurnStarted(match))
                {
                    return BuildFailureResponse(
                        MatchMessageCode.GuessTurnNotStarted,
                        currentState);
                }

                if (GuessTurnPolicy.HasGuessTurnExpired(match))
                {
                    return await ResolveExpiredTurnAsync(
                        unitOfWork,
                        match,
                        playerId,
                        currentState);
                }

                string normalizedLetter =
                    GuessTextNormalizer.Normalize(request.Letter);

                bool letterAlreadyGuessed =
                    await unitOfWork.MatchGuesses.LetterExistsAsync(
                        match.MatchId,
                        normalizedLetter);

                if (letterAlreadyGuessed)
                {
                    return BuildFailureResponse(
                        MatchMessageCode.LetterAlreadyGuessed,
                        currentState);
                }

                LetterGuessResult guessResult =
                    await ProcessLetterGuessAsync(
                        unitOfWork,
                        match,
                        playerId,
                        normalizedLetter);

                if (!guessResult.Success)
                {
                    return BuildFailureResponse(
                        guessResult.MessageCode,
                        currentState,
                        guessResult.IsCorrect);
                }

                return await BuildCommittedGuessResponseAsync(
                    unitOfWork,
                    match.MatchId,
                    playerId,
                    guessResult);
            }
        }

        private static MatchMessageCode ToMatchMessageCode(Enum messageCode)
        {
            if (messageCode is MatchMessageCode matchMessageCode)
            {
                return matchMessageCode;
            }

            return MatchMessageCode.UnexpectedError;
        }

        private static async Task<GuessLetterResponse> ResolveExpiredTurnAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match,
            int playerId,
            MatchGameStateDto currentState)
        {
            bool timeoutResolved =
                await MatchGuessCompletionHelper.FinishWithHostWinAsync(
                    unitOfWork,
                    match);

            if (!timeoutResolved)
            {
                return BuildFailureResponse(
                    MatchMessageCode.UnexpectedError,
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

        private static async Task<LetterGuessResult> ProcessLetterGuessAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match,
            int playerId,
            string normalizedLetter)
        {
            string word = GuessEvaluator.GetWordForPlayer(match, playerId);

            bool isCorrect =
                GuessEvaluator.ContainsLetter(word, normalizedLetter);

            unitOfWork.MatchGuesses.Add(
                CreateMatchGuess(
                    match.MatchId,
                    playerId,
                    normalizedLetter,
                    isCorrect));

            if (isCorrect)
            {
                return await ProcessCorrectLetterAsync(
                    unitOfWork,
                    match,
                    word,
                    normalizedLetter);
            }

            return await ProcessIncorrectLetterAsync(unitOfWork, match);
        }

        private static CreateMatchGuessTransporter CreateMatchGuess(
            int matchId,
            int playerId,
            string normalizedLetter,
            bool isCorrect)
        {
            return new CreateMatchGuessTransporter
            {
                MatchId = matchId,
                GuessedById = playerId,
                Letter = normalizedLetter,
                IsCorrect = isCorrect
            };
        }

        private static async Task<LetterGuessResult> ProcessCorrectLetterAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match,
            string word,
            string normalizedLetter)
        {
            List<string> correctLetters =
                await GetCorrectLettersIncludingCurrentAsync(
                    unitOfWork,
                    match.MatchId,
                    normalizedLetter);

            bool wordCompleted =
                MatchGuessCompletionHelper.IsWordCompleted(
                    word,
                    correctLetters);

            if (wordCompleted)
            {
                return await FinishWithGuesserWinAsync(unitOfWork, match);
            }

            return await UpdateGuessTurnAsync(
                unitOfWork,
                match.MatchId,
                MatchMessageCode.CorrectLetterGuess,
                true);
        }

        private static async Task<List<string>> GetCorrectLettersIncludingCurrentAsync(
            IUnitOfWork unitOfWork,
            int matchId,
            string normalizedLetter)
        {
            List<MatchGuessTransporter> previousGuesses =
                await unitOfWork.MatchGuesses.GetByMatchIdAsync(matchId);

            List<string> correctLetters = previousGuesses
                .Where(guess => guess.IsCorrect)
                .Select(guess => guess.Letter)
                .ToList();

            correctLetters.Add(normalizedLetter);

            return correctLetters;
        }

        private static async Task<LetterGuessResult> ProcessIncorrectLetterAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match)
        {
            bool failedAttemptUpdated =
                await unitOfWork.Matches.IncrementFailedAttemptsAsync(
                    match.MatchId);

            if (!failedAttemptUpdated)
            {
                return LetterGuessResult.Failure(false);
            }

            int updatedFailedAttempts = match.FailedAttempts + 1;

            if (updatedFailedAttempts >= match.MaxAttempts)
            {
                return await FinishWithHostWinAsync(unitOfWork, match);
            }

            return await UpdateGuessTurnAsync(
                unitOfWork,
                match.MatchId,
                MatchMessageCode.IncorrectLetterGuess,
                false);
        }

        private static async Task<LetterGuessResult> FinishWithGuesserWinAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match)
        {
            bool finished =
                await MatchGuessCompletionHelper.FinishWithGuesserWinAsync(
                    unitOfWork,
                    match);

            if (!finished)
            {
                return LetterGuessResult.Failure(true);
            }

            return LetterGuessResult.SuccessResult(
                MatchMessageCode.GuesserWon,
                true,
                true);
        }

        private static async Task<LetterGuessResult> FinishWithHostWinAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match)
        {
            bool finished =
                await MatchGuessCompletionHelper.FinishWithHostWinAsync(
                    unitOfWork,
                    match);

            if (!finished)
            {
                return LetterGuessResult.Failure(false);
            }

            return LetterGuessResult.SuccessResult(
                MatchMessageCode.HostWon,
                false,
                true);
        }

        private static async Task<LetterGuessResult> UpdateGuessTurnAsync(
            IUnitOfWork unitOfWork,
            int matchId,
            MatchMessageCode messageCode,
            bool isCorrect)
        {
            bool turnUpdated =
                await unitOfWork.Matches.UpdateGuessTurnAsync(
                    GuessTurnClock.CreateNextTurn(matchId));

            if (!turnUpdated)
            {
                return LetterGuessResult.Failure(isCorrect);
            }

            return LetterGuessResult.SuccessResult(
                messageCode,
                isCorrect,
                false);
        }

        private static async Task<GuessLetterResponse> BuildCommittedGuessResponseAsync(
            IUnitOfWork unitOfWork,
            int matchId,
            int playerId,
            LetterGuessResult guessResult)
        {
            await unitOfWork.CommitAsync();

            MatchGameStateDto updatedState =
                await MatchGameStateLoader.BuildByMatchIdAsync(
                    unitOfWork,
                    matchId,
                    playerId);

            return MatchGuessResponseFactory.BuildGuessLetterResponse(
                true,
                guessResult.MessageCode,
                guessResult.IsCorrect,
                guessResult.MatchFinished,
                updatedState);
        }

        private static GuessLetterResponse BuildFailureResponse(
            MatchMessageCode messageCode,
            MatchGameStateDto gameState,
            bool isCorrect = false)
        {
            return MatchGuessResponseFactory.BuildGuessLetterResponse(
                false,
                messageCode,
                isCorrect,
                false,
                gameState);
        }

        private sealed class LetterGuessResult
        {
            private LetterGuessResult(
                bool success,
                MatchMessageCode messageCode,
                bool isCorrect,
                bool matchFinished)
            {
                Success = success;
                MessageCode = messageCode;
                IsCorrect = isCorrect;
                MatchFinished = matchFinished;
            }

            public bool Success { get; private set; }

            public MatchMessageCode MessageCode { get; private set; }

            public bool IsCorrect { get; private set; }

            public bool MatchFinished { get; private set; }

            public static LetterGuessResult SuccessResult(
                MatchMessageCode messageCode,
                bool isCorrect,
                bool matchFinished)
            {
                return new LetterGuessResult(
                    true,
                    messageCode,
                    isCorrect,
                    matchFinished);
            }

            public static LetterGuessResult Failure(bool isCorrect)
            {
                return new LetterGuessResult(
                    false,
                    MatchMessageCode.UnexpectedError,
                    isCorrect,
                    false);
            }
        }
    }
}
