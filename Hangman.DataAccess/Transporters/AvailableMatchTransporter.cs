using System;

namespace Hangman.DataAccess.Transporters
{
    public class AvailableMatchTransporter
    {
        public int MatchId { get; set; }

        public int HostId { get; set; }

        public string HostFullName { get; set; }

        public string HostEmail { get; set; }

        public string CategoryName { get; set; }

        public string LanguageCode { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
