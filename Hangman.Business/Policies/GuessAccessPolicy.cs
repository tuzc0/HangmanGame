using Hangman.Business.Constants;
using Hangman.Business.Messages;
using Hangman.DataAccess.Transporters;

namespace Hangman.Business.Policies
{
    internal static class GuessAccessPolicy
    {
        public static MatchMessageCode? ValidateGameStateAccess(
            MatchTransporter match,
            int playerId)
        {
            if (match == null)
            {
                return MatchMessageCode.MatchNotFound;
            }

            if (!PlayerBelongsToMatch(match, playerId))
            {
                return MatchMessageCode.PlayerNotInMatch;
            }

            return null;
        }

        public static MatchMessageCode? ValidateGuestGuessAccess(
            MatchTransporter match,
            int playerId)
        {
            MatchMessageCode? accessValidation =
                ValidateGameStateAccess(match, playerId);

            if (accessValidation.HasValue)
            {
                return accessValidation.Value;
            }

            if (match.MatchStatus != MatchStatusConstants.InProgress)
            {
                return MatchMessageCode.GameNotInProgress;
            }

            if (!match.GuestId.HasValue || match.GuestId.Value != playerId)
            {
                return MatchMessageCode.GuessNotAllowed;
            }

            return null;
        }

        private static bool PlayerBelongsToMatch(
            MatchTransporter match,
            int playerId)
        {
            return match.HostId == playerId ||
                   match.GuestId == playerId;
        }
    }
}
