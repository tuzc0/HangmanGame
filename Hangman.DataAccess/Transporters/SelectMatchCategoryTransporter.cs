using System;

namespace Hangman.DataAccess.Transporters
{
    public class SelectMatchCategoryTransporter
    {
        public int MatchId { get; set; }

        public int SelectedCategoryId { get; set; }

        public string MatchStatus { get; set; }

        public DateTime? WordSelectionStartedAt { get; set; }

        public DateTime? WordSelectionEndsAt { get; set; }
    }
}
