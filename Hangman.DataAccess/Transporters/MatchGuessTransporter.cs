using System;

namespace Hangman.DataAccess.Transporters
{
    public class MatchGuessTransporter
    {
        public int GuessId { get; set; }

        public int MatchId { get; set; }

        public int GuessedById { get; set; }

        public string Letter { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
