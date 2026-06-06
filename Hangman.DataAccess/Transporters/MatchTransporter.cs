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

        public int WordId { get; set; }

        public string WordText { get; set; }

        public string WordDescription { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string LanguageCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? JoinedAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public string MatchStatus { get; set; }

        public int? WinnerId { get; set; }

        public int? PenalizedUserId { get; set; }

        public int FailedAttempts { get; set; }

        public int MaxAttempts { get; set; }
    }
}