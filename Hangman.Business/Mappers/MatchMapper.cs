using Hangman.Contracts.Match;
using Hangman.DataAccess.Transporters;

namespace Hangman.Business.Mappers
{
    internal static class MatchMapper
    {
        public static MatchLobbyDto ToMatchLobbyDto(MatchTransporter match)
        {
            if (match == null)
            {
                return null;
            }

            return new MatchLobbyDto
            {
                MatchId = match.MatchId,
                HostId = match.HostId,
                HostFullName = match.HostFullName,
                HostLanguageCode = match.HostLanguageCode,
                GuestId = match.GuestId,
                GuestFullName = match.GuestFullName,
                GuestLanguageCode = match.GuestLanguageCode,

                SelectedCategoryId = match.SelectedCategoryId,
                SelectedCategoryName = match.HostCategoryName,
                SelectedWordId = match.SelectedWordId,
                WordSelectionStartedAt = match.WordSelectionStartedAt,
                WordSelectionEndsAt = match.WordSelectionEndsAt,
                StartedAt = match.StartedAt,
                FinishedAt = match.FinishedAt,
                WinnerId = match.WinnerId,
                PenalizedUserId = match.PenalizedUserId,

                MatchStatus = match.MatchStatus,
                CreatedAt = match.CreatedAt,
                JoinedAt = match.JoinedAt,
                CategoryVotingStartedAt = match.CategoryVotingStartedAt,
                CategoryVotingEndsAt = match.CategoryVotingEndsAt
            };
        }

        public static AvailableLobbyDto ToAvailableLobbyDto(
            AvailableMatchTransporter match)
        {
            if (match == null)
            {
                return null;
            }

            return new AvailableLobbyDto
            {
                MatchId = match.MatchId,
                HostId = match.HostId,
                HostFullName = match.HostFullName,
                HostEmail = match.HostEmail,
                HostLanguageCode = match.HostLanguageCode,
                MatchStatus = match.MatchStatus,
                CreatedAt = match.CreatedAt
            };
        }
    }
}
