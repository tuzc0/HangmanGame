namespace Hangman.DataAccess.Transporters
{
    public class CreateMatchGuessTransporter
    {
        public int MatchId { get; set; }

        public int GuessedById { get; set; }

        public string Letter { get; set; }

        public bool IsCorrect { get; set; }
    }
}
