using System;

namespace Hangman.DataAccess.Transporters
{
    public class AccountTransporter
    {
        public int AccountId { get; set; }

        public int PlayerId { get; set; }

        public string Email { get; set; }

        public bool IsEmailVerified { get; set; }

        public DateTime? EmailVerifiedAt { get; set; }

        public string AccountStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
