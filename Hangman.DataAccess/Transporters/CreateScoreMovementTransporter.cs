namespace Hangman.DataAccess.Transporters
{
    public class CreateScoreMovementTransporter
    {
        public int PlayerId { get; set; }

        public int MatchId { get; set; }

        public int Points { get; set; }

        public string MovementType { get; set; }
    }
}
