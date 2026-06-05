using System;

namespace Hangman.DataAccess.Transporters
{
    public class CreateAccountTransporter
    {
        public int PlayerId { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public bool IsEmailVerified { get; set; }

        public DateTime? EmailVerifiedAt { get; set; }

        public string AccountStatus { get; set; }
    }
}
