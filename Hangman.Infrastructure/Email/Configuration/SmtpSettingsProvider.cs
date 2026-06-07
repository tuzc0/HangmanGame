using System;
using System.Configuration;

namespace Hangman.Infrastructure.Email.Configuration
{
    public class SmtpSettingsProvider
    {
        private const int DefaultPort = 587;
        private const int DefaultTimeoutMs = 10000;
        private const string DefaultDisplayName = "Hangman Game";

        public SmtpSettings GetSettings()
        {
            return new SmtpSettings
            {
                Host = GetString("Smtp.Host", "HANGMAN_SMTP_HOST", string.Empty),
                Port = GetInt("Smtp.Port", "HANGMAN_SMTP_PORT", DefaultPort),
                User = GetString("Smtp.User", "HANGMAN_SMTP_USER", string.Empty),
                Password = GetString("Smtp.Password", "HANGMAN_SMTP_PASSWORD", string.Empty),
                FromAddress = GetString("Smtp.FromAddress", "HANGMAN_SMTP_FROM_ADDRESS", string.Empty),
                DisplayName = GetString("Smtp.DisplayName", "HANGMAN_SMTP_DISPLAY_NAME", DefaultDisplayName),
                TimeoutMs = GetInt("Smtp.TimeoutMs", "HANGMAN_SMTP_TIMEOUT_MS", DefaultTimeoutMs)
            };
        }

        private static string GetString(string appSettingKey, string environmentVariableKey, string defaultValue)
        {
            string value = GetRawValue(appSettingKey, environmentVariableKey);

            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return value.Trim();
        }

        private static int GetInt(string appSettingKey, string environmentVariableKey, int defaultValue)
        {
            string value = GetRawValue(appSettingKey, environmentVariableKey);

            int parsedValue;

            if (int.TryParse(value, out parsedValue) && parsedValue > 0)
            {
                return parsedValue;
            }

            return defaultValue;
        }

        private static string GetRawValue(string appSettingKey, string environmentVariableKey)
        {
            string environmentValue = Environment.GetEnvironmentVariable(environmentVariableKey);

            if (!string.IsNullOrWhiteSpace(environmentValue))
            {
                return environmentValue;
            }

            return ConfigurationManager.AppSettings[appSettingKey];
        }
    }
}
