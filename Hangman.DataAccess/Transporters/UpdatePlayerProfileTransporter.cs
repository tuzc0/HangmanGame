using System;

namespace Hangman.DataAccess.Transporters
{
    public class UpdatePlayerProfileTransporter
    {
        public int PlayerId { get; set; }

        public string FullName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Phone { get; set; }

        public string PreferredLanguageCode { get; set; }
    }
}
