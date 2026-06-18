using Hangman.Contracts.Match;
using System;

namespace Hangman.Business.Mappers
{
    internal static class MatchGuessResponseFactory
    {
        public static GetMatchGameStateResponse BuildGetMatchGameStateResponse(
            bool success,
            Enum messageCode,
            MatchGameStateDto gameState)
        {
            return new GetMatchGameStateResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                GameState = gameState
            };
        }

        public static GuessLetterResponse BuildGuessLetterResponse(
            bool success,
            Enum messageCode,
            bool isCorrect,
            bool matchFinished,
            MatchGameStateDto gameState)
        {
            return new GuessLetterResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                IsCorrect = isCorrect,
                MatchFinished = matchFinished,
                GameState = gameState
            };
        }

        public static GuessWordResponse BuildGuessWordResponse(
            bool success,
            Enum messageCode,
            bool isCorrect,
            bool matchFinished,
            MatchGameStateDto gameState)
        {
            return new GuessWordResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                IsCorrect = isCorrect,
                MatchFinished = matchFinished,
                GameState = gameState
            };
        }

        public static ResolveGuessTimeoutResponse BuildResolveGuessTimeoutResponse(
            bool success,
            Enum messageCode,
            bool matchFinished,
            MatchGameStateDto gameState)
        {
            return new ResolveGuessTimeoutResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                MatchFinished = matchFinished,
                GameState = gameState
            };
        }
    }
}
