namespace Hangman.DataAccess.Transporters
{
    public class FinishMatchTransporter
    {
        public int MatchId { get; set; }

        public int? WinnerId { get; set; }

        public string MatchStatus { get; set; }
    }
}
