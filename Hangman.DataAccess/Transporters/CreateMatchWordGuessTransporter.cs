namespace Hangman.DataAccess.Transporters
{
    public class CreateMatchWordGuessTransporter
    {
        public int MatchId { get; set; }

        public int GuessedById { get; set; }

        public string GuessedWord { get; set; }

        public bool IsCorrect { get; set; }
    }
}
