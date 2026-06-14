using Hangman.Business.Constants;
using Hangman.DataAccess.Transporters;
using System;

namespace Hangman.Business.Policies
{
    internal static class LobbyLeavePolicy
    {
        public static bool PlayerBelongsToMatch(
            MatchTransporter match,
            int playerId)
        {
            if (match == null)
            {
                return false;
            }

            return match.HostId == playerId ||
                   match.GuestId == playerId;
        }

        public static bool CanLeaveWithoutPenalty(MatchTransporter match)
        {
            if (match == null)
            {
                return false;
            }

            if (match.MatchStatus == MatchStatusConstants.WaitingForGuest)
            {
                return true;
            }

            if (match.MatchStatus == MatchStatusConstants.VotingCategory)
            {
                DateTime safeLeaveDeadline = GetSafeLeaveDeadline(match);

                return DateTime.UtcNow <= safeLeaveDeadline;
            }

            return false;
        }

        private static DateTime GetSafeLeaveDeadline(MatchTransporter match)
        {
            DateTime baseDate = match.CategoryVotingStartedAt
                ?? match.JoinedAt
                ?? match.CreatedAt;

            return baseDate.AddSeconds(MatchTimingConstants.SafeLeaveDurationSeconds);
        }
    }
}
