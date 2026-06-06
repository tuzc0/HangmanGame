namespace Hangman.DataAccess.Transporters
{
    public class AbandonMatchTransporter
    {
        public int MatchId { get; set; }

        public int PenalizedUserId { get; set; }

        public string MatchStatus { get; set; }
    }
}
