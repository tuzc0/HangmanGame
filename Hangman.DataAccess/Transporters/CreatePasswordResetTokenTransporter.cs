using System;

namespace Hangman.DataAccess.Transporters
{
    public class CreatePasswordResetTokenTransporter
    {
        public int AccountId { get; set; }

        public string ResetCodeHash { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
