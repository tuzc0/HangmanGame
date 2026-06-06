using System;

namespace Hangman.DataAccess.Transporters
{
    public class ScoreMovementTransporter
    {
        public int ScoreMovementId { get; set; }

        public int PlayerId { get; set; }

        public int MatchId { get; set; }

        public int Points { get; set; }

        public string MovementType { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
