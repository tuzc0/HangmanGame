using System;

namespace Hangman.DataAccess.Transporters
{
    public class PasswordResetTokenTransporter
    {
        public int PasswordResetTokenId { get; set; }

        public int AccountId { get; set; }

        public string ResetCodeHash { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public int Attempts { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
