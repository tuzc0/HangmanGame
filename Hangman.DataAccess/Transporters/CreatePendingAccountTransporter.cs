using System;

namespace Hangman.DataAccess.Transporters
{
    public class CreatePendingAccountTransporter
    {
        public string FullName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Phone { get; set; }

        public bool IsPlayerActive { get; set; }

        public string PreferredLanguageCode { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public bool IsEmailVerified { get; set; }

        public DateTime? EmailVerifiedAt { get; set; }

        public string AccountStatus { get; set; }

        public string VerificationCodeHash { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
