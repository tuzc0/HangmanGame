using Hangman.Business.Constants;
using Hangman.DataAccess.Transporters;
using System;

namespace Hangman.Business.Policies
{
    internal static class WordSelectionPolicy
    {
        public static bool HasWordSelectionExpired(MatchTransporter match)
        {
            return match != null &&
                   match.WordSelectionEndsAt.HasValue &&
                   DateTime.UtcNow > match.WordSelectionEndsAt.Value;
        }

        public static bool CanCurrentPlayerSelectWord(
            MatchTransporter match,
            int currentPlayerId)
        {
            return match != null &&
                   match.MatchStatus == MatchStatusConstants.WaitingForHostWord &&
                   match.HostId == currentPlayerId;
        }

        public static bool IsWordSelectionActive(MatchTransporter match)
        {
            return match != null &&
                   match.MatchStatus == MatchStatusConstants.WaitingForHostWord &&
                   match.SelectedCategoryId.HasValue;
        }
    }
}
