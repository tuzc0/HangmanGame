using Hangman.DataAccess.Transporters;

namespace Hangman.Business.Helpers
{
    internal static class MatchWinnerResolver
    {
        public static int GetWinnerPlayerId(
            MatchTransporter match,
            int penalizedPlayerId)
        {
            if (match == null)
            {
                return 0;
            }

            if (match.HostId == penalizedPlayerId)
            {
                return match.GuestId ?? 0;
            }

            if (match.GuestId.HasValue &&
                match.GuestId.Value == penalizedPlayerId)
            {
                return match.HostId;
            }

            return 0;
        }
    }
}
