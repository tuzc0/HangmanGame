using System;

namespace Hangman.DataAccess.Transporters
{
    public class JoinMatchTransporter
    {
        public int MatchId { get; set; }

        public int GuestId { get; set; }

        public string GuestLanguageCode { get; set; }

        public string MatchStatus { get; set; }

        public DateTime CategoryVotingStartedAt { get; set; }

        public DateTime CategoryVotingEndsAt { get; set; }
    }
}
