using System;

namespace Hangman.DataAccess.Transporters
{
    public class SelectMatchWordTransporter
    {
        public int MatchId { get; set; }

        public int SelectedWordId { get; set; }

        public string MatchStatus { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? GuessTurnStartedAt { get; set; }

        public DateTime? GuessTurnEndsAt { get; set; }
    }
}
