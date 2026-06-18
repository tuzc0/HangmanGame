using System;

namespace Hangman.DataAccess.Transporters
{
    public class MatchTransporter
    {
        public int MatchId { get; set; }

        public int HostId { get; set; }

        public string HostFullName { get; set; }

        public string HostEmail { get; set; }

        public int? GuestId { get; set; }

        public string GuestFullName { get; set; }

        public string GuestEmail { get; set; }

        public string HostLanguageCode { get; set; }

        public string GuestLanguageCode { get; set; }

        public int? SelectedCategoryId { get; set; }

        public string HostCategoryName { get; set; }

        public string GuestCategoryName { get; set; }

        public int? SelectedWordId { get; set; }

        public string HostWordText { get; set; }

        public string GuestWordText { get; set; }

        public string HostWordDescription { get; set; }

        public string GuestWordDescription { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? JoinedAt { get; set; }

        public DateTime? CategoryVotingStartedAt { get; set; }

        public DateTime? CategoryVotingEndsAt { get; set; }

        public DateTime? WordSelectionStartedAt { get; set; }

        public DateTime? WordSelectionEndsAt { get; set; }

        public DateTime? GuessTurnStartedAt { get; set; }

        public DateTime? GuessTurnEndsAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public string MatchStatus { get; set; }

        public int? WinnerId { get; set; }

        public int? PenalizedUserId { get; set; }

        public int FailedAttempts { get; set; }

        public int MaxAttempts { get; set; }
    }
}