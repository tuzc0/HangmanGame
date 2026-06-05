using System;

namespace Hangman.DataAccess.Transporters
{
    public class CreateEmailVerificationTransporter
    {
        public int AccountId { get; set; }

        public string VerificationCodeHash { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
