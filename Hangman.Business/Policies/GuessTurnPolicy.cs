using Hangman.DataAccess.Transporters;
using System;

namespace Hangman.Business.Policies
{
    internal static class GuessTurnPolicy
    {
        public static bool HasGuessTurnStarted(MatchTransporter match)
        {
            return match != null &&
                   match.GuessTurnStartedAt.HasValue &&
                   match.GuessTurnEndsAt.HasValue;
        }

        public static bool HasGuessTurnExpired(MatchTransporter match)
        {
            return match == null ||
                   !match.GuessTurnEndsAt.HasValue ||
                   DateTime.UtcNow > match.GuessTurnEndsAt.Value;
        }

        public static int GetRemainingSeconds(MatchTransporter match)
        {
            if (match == null || !match.GuessTurnEndsAt.HasValue)
            {
                return 0;
            }

            double remainingSeconds =
                (match.GuessTurnEndsAt.Value - DateTime.UtcNow).TotalSeconds;

            if (remainingSeconds <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(remainingSeconds);
        }
    }
}
