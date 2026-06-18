using Hangman.Business.Constants;
using Hangman.Business.Helpers;
using Hangman.Business.Policies;
using Hangman.Contracts.Match;
using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Linq;

namespace Hangman.Business.Mappers
{
    internal static class MatchGameStateMapper
    {
        public static MatchGameStateDto ToGameStateDto(
            MatchTransporter match,
            List<MatchGuessTransporter> letterGuesses,
            List<MatchWordGuessTransporter> wordGuesses,
            int currentPlayerId)
        {
            if (match == null)
            {
                return null;
            }

            List<MatchGuessTransporter> safeLetterGuesses =
                letterGuesses ?? new List<MatchGuessTransporter>();

            List<MatchWordGuessTransporter> safeWordGuesses =
                wordGuesses ?? new List<MatchWordGuessTransporter>();

            string word = GuessEvaluator.GetWordForPlayer(match, currentPlayerId);

            bool isCurrentPlayerHost = match.HostId == currentPlayerId;

            bool isMatchResolved =
                match.MatchStatus == MatchStatusConstants.Finished ||
                match.MatchStatus == MatchStatusConstants.Abandoned ||
                match.MatchStatus == MatchStatusConstants.Cancelled;

            bool revealAll = isCurrentPlayerHost || isMatchResolved;

            string wordDescription = isCurrentPlayerHost
                ? match.HostWordDescription
                : match.GuestWordDescription;

            List<string> correctLetters = safeLetterGuesses
                .Where(guess => guess.IsCorrect)
                .Select(guess => guess.Letter)
                .ToList();

            string winnerFullName = GetWinnerFullName(match);
            string winnerEmail = GetWinnerEmail(match);

            return new MatchGameStateDto
            {
                MatchId = match.MatchId,
                HostId = match.HostId,
                HostFullName = match.HostFullName,
                GuestId = match.GuestId,
                GuestFullName = match.GuestFullName,
                MatchStatus = match.MatchStatus,
                FailedAttempts = match.FailedAttempts,
                MaxAttempts = match.MaxAttempts,
                GuessTurnStartedAt = match.GuessTurnStartedAt,
                GuessTurnEndsAt = match.GuessTurnEndsAt,
                RemainingSeconds = GuessTurnPolicy.GetRemainingSeconds(match),
                IsFinished = isMatchResolved,
                WinnerId = match.WinnerId,
                WinnerFullName = winnerFullName,
                WinnerEmail = winnerEmail,
                LetterSlots = WordProgressBuilder.Build(
                    word,
                    correctLetters,
                    revealAll),
                WordDescription = wordDescription,
                GuessHistory = BuildGuessHistory(
                    safeLetterGuesses,
                    safeWordGuesses),
                HangmanFigure = HangmanFigureBuilder.Build(
                    match.FailedAttempts,
                    match.MaxAttempts)
            };
        }

        private static List<GuessHistoryDto> BuildGuessHistory(
            List<MatchGuessTransporter> letterGuesses,
            List<MatchWordGuessTransporter> wordGuesses)
        {
            List<GuessHistoryDto> history = new List<GuessHistoryDto>();

            history.AddRange(letterGuesses.Select(guess => new GuessHistoryDto
            {
                GuessType = GuessConstants.LetterGuessType,
                Value = guess.Letter,
                IsCorrect = guess.IsCorrect,
                CreatedAt = guess.CreatedAt
            }));

            history.AddRange(wordGuesses.Select(guess => new GuessHistoryDto
            {
                GuessType = GuessConstants.WordGuessType,
                Value = guess.GuessedWord,
                IsCorrect = guess.IsCorrect,
                CreatedAt = guess.CreatedAt
            }));

            return history
                .OrderBy(item => item.CreatedAt)
                .ToList();
        }

        private static string GetWinnerFullName(MatchTransporter match)
        {
            if (match == null || !match.WinnerId.HasValue)
            {
                return string.Empty;
            }

            if (match.WinnerId.Value == match.HostId)
            {
                return match.HostFullName;
            }

            if (match.GuestId.HasValue &&
                match.WinnerId.Value == match.GuestId.Value)
            {
                return match.GuestFullName;
            }

            return string.Empty;
        }

        private static string GetWinnerEmail(MatchTransporter match)
        {
            if (match == null || !match.WinnerId.HasValue)
            {
                return string.Empty;
            }

            if (match.WinnerId.Value == match.HostId)
            {
                return match.HostEmail;
            }

            if (match.GuestId.HasValue &&
                match.WinnerId.Value == match.GuestId.Value)
            {
                return match.GuestEmail;
            }

            return string.Empty;
        }
    }
}
