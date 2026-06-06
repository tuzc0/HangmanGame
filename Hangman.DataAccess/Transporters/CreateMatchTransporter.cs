namespace Hangman.DataAccess.Transporters
{
    public class CreateMatchTransporter
    {
        public int HostId { get; set; }

        public int WordId { get; set; }

        public string MatchStatus { get; set; }

        public int FailedAttempts { get; set; }

        public int MaxAttempts { get; set; }
    }
}
