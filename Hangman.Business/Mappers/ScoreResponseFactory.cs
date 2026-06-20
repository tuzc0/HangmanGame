using Hangman.Contracts.Match;
using System;
using System.Collections.Generic;

namespace Hangman.Business.Mappers
{
    internal static class ScoreResponseFactory
    {
        public static GetPlayerScoreResponse BuildGetPlayerScoreResponse(
            bool success,
            Enum messageCode,
            int totalScore,
            int guesserWinTotal,
            int guesserWinCount,
            int hostWinTotal,
            int hostWinCount,
            int penaltyTotal,
            int penaltyCount,
            List<ScoreMovementDto> movements)
        {
            return new GetPlayerScoreResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                TotalScore = totalScore,
                GuesserWinTotal = guesserWinTotal,
                GuesserWinCount = guesserWinCount,
                HostWinTotal = hostWinTotal,
                HostWinCount = hostWinCount,
                PenaltyTotal = penaltyTotal,
                PenaltyCount = penaltyCount,
                Movements = movements
            };
        }

        public static GetPlayerScoreResponse BuildFailResponse(Enum messageCode)
        {
            return new GetPlayerScoreResponse
            {
                Success = false,
                MessageCode = messageCode.ToString(),
                TotalScore = 0,
                GuesserWinTotal = 0,
                GuesserWinCount = 0,
                HostWinTotal = 0,
                HostWinCount = 0,
                PenaltyTotal = 0,
                PenaltyCount = 0,
                Movements = null
            };
        }
    }
}
