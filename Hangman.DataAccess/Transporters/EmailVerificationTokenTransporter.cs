using System;

namespace Hangman.DataAccess.Transporters
{
    public class EmailVerificationTokenTransporter
    {
        public int EmailVerificationId { get; set; }

        public int AccountId { get; set; }

        public string VerificationCodeHash { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public int Attempts { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
