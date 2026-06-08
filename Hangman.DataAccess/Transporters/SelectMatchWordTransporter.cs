namespace Hangman.DataAccess.Transporters
{
    public class SelectMatchWordTransporter
    {
        public int MatchId { get; set; }

        public int SelectedWordId { get; set; }

        public string MatchStatus { get; set; }
    }
}
