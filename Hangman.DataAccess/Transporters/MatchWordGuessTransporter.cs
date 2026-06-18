using System;

namespace Hangman.DataAccess.Transporters
{
    public class MatchWordGuessTransporter
    {
        public int WordGuessId { get; set; }

        public int MatchId { get; set; }

        public int GuessedById { get; set; }

        public string GuessedWord { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
