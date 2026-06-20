namespace Hangman.ConsoleHost.Configuration
{
    public class HostingSettings
    {
        public string BaseAddress { get; set; }

        public string DuplexBaseAddress { get; set; }

        public string AuthServicePath { get; set; }

        public string ProfileServicePath { get; set; }

        public string WordServicePath { get; set; }

        public string MatchServicePath { get; set; }

        public string MatchGameplayServicePath { get; set; }

        public string MatchGuessServicePath { get; set; }

        public string MatchNotificationServicePath { get; set; }

        public string MatchChatServicePath { get; set; }

        public string ScoreServicePath { get; set; }

        public long MaxReceivedMessageSize { get; set; }

        public int OpenTimeoutSeconds { get; set; }

        public int CloseTimeoutSeconds { get; set; }

        public int SendTimeoutSeconds { get; set; }

        public int ReceiveTimeoutMinutes { get; set; }

        public bool MetadataEnabled { get; set; }

        public bool IncludeExceptionDetailInFaults { get; set; }
    }
}
