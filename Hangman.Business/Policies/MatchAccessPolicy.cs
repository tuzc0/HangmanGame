using Hangman.Business.Constants;
using Hangman.Business.Messages;
using Hangman.DataAccess.Transporters;

namespace Hangman.Business.Policies
{
    internal static class MatchAccessPolicy
    {
        public static MatchMessageCode? ValidateMatchForPlayer(
            MatchTransporter match,
            int playerId)
        {
            if (match == null)
            {
                return MatchMessageCode.MatchNotFound;
            }

            bool playerBelongsToMatch =
                match.HostId == playerId ||
                match.GuestId == playerId;

            if (!playerBelongsToMatch)
            {
                return MatchMessageCode.PlayerNotInMatch;
            }

            if (match.MatchStatus == MatchStatusConstants.Finished ||
                match.MatchStatus == MatchStatusConstants.Abandoned ||
                match.MatchStatus == MatchStatusConstants.Cancelled)
            {
                return MatchMessageCode.MatchAlreadyResolved;
            }

            return null;
        }
    }
}
