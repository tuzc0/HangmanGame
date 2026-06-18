using Hangman.Business.Constants;
using Hangman.DataAccess.Transporters;
using System;

namespace Hangman.Business.Helpers
{
    internal static class GuessTurnClock
    {
        public static UpdateGuessTurnTransporter CreateNextTurn(
            int matchId)
        {
            DateTime startedAt = DateTime.UtcNow;

            return new UpdateGuessTurnTransporter
            {
                MatchId = matchId,
                GuessTurnStartedAt = startedAt,
                GuessTurnEndsAt = startedAt.AddSeconds(
                    GuessConstants.GuessTurnDurationSeconds)
            };
        }
    }
}
