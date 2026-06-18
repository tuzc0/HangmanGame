using System;

namespace Hangman.DataAccess.Transporters
{
    public class UpdateGuessTurnTransporter
    {
        public int MatchId { get; set; }

        public DateTime GuessTurnStartedAt { get; set; }

        public DateTime GuessTurnEndsAt { get; set; }
    }
}
