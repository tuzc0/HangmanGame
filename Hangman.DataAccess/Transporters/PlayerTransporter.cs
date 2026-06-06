using System;

namespace Hangman.DataAccess.Transporters
{
    public class PlayerTransporter
    {
        public int PlayerId { get; set; }

        public string FullName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Phone { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsActive { get; set; }

        public string PreferredLanguageCode { get; set; }
    }
}
