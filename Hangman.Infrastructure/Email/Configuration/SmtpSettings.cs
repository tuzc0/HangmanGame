namespace Hangman.Infrastructure.Email.Configuration
{
    public class SmtpSettings
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public bool EnableSsl { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string FromAddress { get; set; }

        public string DisplayName { get; set; }

        public int TimeoutMs { get; set; }

        public bool TryValidate()
        {
            return !string.IsNullOrWhiteSpace(Host) &&
                   Port > 0 &&
                   !string.IsNullOrWhiteSpace(User) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(FromAddress) &&
                   TimeoutMs > 0;
        }
    }
}
