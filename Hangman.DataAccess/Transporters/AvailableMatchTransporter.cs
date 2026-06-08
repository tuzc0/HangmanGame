using System;

namespace Hangman.DataAccess.Transporters
{
    public class AvailableMatchTransporter
    {
        public int MatchId { get; set; }

        public int HostId { get; set; }

        public string HostFullName { get; set; }

        public string HostEmail { get; set; }

        public string HostLanguageCode { get; set; }

        public string MatchStatus { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
