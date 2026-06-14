namespace Hangman.DataAccess.Transporters
{
    public class SaveMatchCategoryVoteTransporter
    {
        public int MatchId { get; set; }

        public int PlayerId { get; set; }

        public int CategoryId { get; set; }
    }
}
