using System.Configuration;

namespace Hangman.ConsoleHost.Configuration
{
    public class HostingSettingsProvider
    {
        private const string DefaultBaseAddress = "http://localhost:8085/Hangman/";
        private const string DefaultDuplexBaseAddress = "net.tcp://localhost:8090/Hangman/";
        private const string DefaultMatchNotificationServicePath = "MatchNotificationService";
        private const string DefaultAuthServicePath = "AuthService/";
        private const long DefaultMaxReceivedMessageSize = 65536;
        private const int DefaultOpenTimeoutSeconds = 30;
        private const int DefaultCloseTimeoutSeconds = 30;
        private const int DefaultSendTimeoutSeconds = 30;
        private const int DefaultReceiveTimeoutMinutes = 10;
        private const bool DefaultMetadataEnabled = true;
        private const bool DefaultIncludeExceptionDetailInFaults = false;

        public HostingSettings GetSettings()
        {
            return new HostingSettings
            {
                BaseAddress = GetString("Wcf.BaseAddress", DefaultBaseAddress),
                DuplexBaseAddress = GetString("Wcf.DuplexBaseAddress", DefaultDuplexBaseAddress),
                MatchNotificationServicePath = GetString("Wcf.MatchNotificationServicePath", DefaultMatchNotificationServicePath),
                AuthServicePath = GetString("Wcf.AuthServicePath", DefaultAuthServicePath),
                ProfileServicePath = GetString("Wcf.ProfileServicePath", "ProfileService"),
                WordServicePath = GetString("Wcf.WordServicePath", "WordService"),
                MatchServicePath = GetString("Wcf.MatchServicePath", "MatchService"),
                MaxReceivedMessageSize = GetLong("Wcf.MaxReceivedMessageSize", DefaultMaxReceivedMessageSize),
                OpenTimeoutSeconds = GetInt("Wcf.OpenTimeoutSeconds", DefaultOpenTimeoutSeconds),
                CloseTimeoutSeconds = GetInt("Wcf.CloseTimeoutSeconds", DefaultCloseTimeoutSeconds),
                SendTimeoutSeconds = GetInt("Wcf.SendTimeoutSeconds", DefaultSendTimeoutSeconds),
                ReceiveTimeoutMinutes = GetInt("Wcf.ReceiveTimeoutMinutes", DefaultReceiveTimeoutMinutes),
                MetadataEnabled = GetBool("Wcf.MetadataEnabled", DefaultMetadataEnabled),
                IncludeExceptionDetailInFaults = GetBool("Wcf.IncludeExceptionDetailInFaults", DefaultIncludeExceptionDetailInFaults)
            };
        }

        private static string GetString(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return value.Trim();
        }

        private static int GetInt(string key, int defaultValue)
        {
            int value;

            if (int.TryParse(ConfigurationManager.AppSettings[key], out value) && value > 0)
            {
                return value;
            }

            return defaultValue;
        }

        private static long GetLong(string key, long defaultValue)
        {
            long value;

            if (long.TryParse(ConfigurationManager.AppSettings[key], out value) && value > 0)
            {
                return value;
            }

            return defaultValue;
        }

        private static bool GetBool(string key, bool defaultValue)
        {
            bool value;

            if (bool.TryParse(ConfigurationManager.AppSettings[key], out value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}
