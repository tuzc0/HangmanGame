using System;

namespace Hangman.DataAccess.Transporters
{
    public class MatchCategoryVoteTransporter
    {
        public int MatchCategoryVoteId { get; set; }

        public int MatchId { get; set; }

        public int PlayerId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string LanguageCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
