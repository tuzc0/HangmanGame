namespace Hangman.DataAccess.Transporters
{
    public class AccountCredentialsTransporter
    {
        public int AccountId { get; set; }

        public int PlayerId { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public bool IsEmailVerified { get; set; }

        public string AccountStatus { get; set; }
    }
}
