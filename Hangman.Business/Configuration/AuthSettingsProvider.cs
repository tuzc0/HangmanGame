using System;
using System.Configuration;
using System.Linq;

namespace Hangman.Business.Configuration
{
    public class AuthSettingsProvider
    {
        private const int DefaultPasswordSaltSize = 16;
        private const int DefaultPasswordHashSize = 32;
        private const int DefaultPasswordIterations = 10000;
        private const int DefaultPasswordResetExpirationMinutes = 15;
        private const int DefaultVerificationCodeLength = 6;
        private const int DefaultVerificationCodeLimit = 1000000;
        private const int DefaultEmailVerificationExpirationMinutes = 15;
        private const int DefaultMinimumPasswordLength = 8;
        private const int DefaultMaximumVerificationAttempts = 5;
        private const string DefaultLanguageCodeValue = "es";
        private const string DefaultAllowedLanguageCodesValue = "es,en";

        public AuthSettings GetSettings()
        {
            return new AuthSettings
            {
                PasswordSaltSize = GetInt("Auth.PasswordSaltSize", "HANGMAN_AUTH_PASSWORD_SALT_SIZE", DefaultPasswordSaltSize),
                PasswordHashSize = GetInt("Auth.PasswordHashSize", "HANGMAN_AUTH_PASSWORD_HASH_SIZE", DefaultPasswordHashSize),
                PasswordIterations = GetInt("Auth.PasswordIterations", "HANGMAN_AUTH_PASSWORD_ITERATIONS", DefaultPasswordIterations),
                PasswordResetExpirationMinutes = GetInt("Auth.PasswordResetExpirationMinutes", "HANGMAN_AUTH_PASSWORD_RESET_EXPIRATION_MINUTES", DefaultPasswordResetExpirationMinutes),
                VerificationCodeLength = GetInt("Auth.VerificationCodeLength", "HANGMAN_AUTH_VERIFICATION_CODE_LENGTH", DefaultVerificationCodeLength),
                VerificationCodeLimit = GetInt("Auth.VerificationCodeLimit", "HANGMAN_AUTH_VERIFICATION_CODE_LIMIT", DefaultVerificationCodeLimit),
                EmailVerificationExpirationMinutes = GetInt("Auth.EmailVerificationExpirationMinutes", "HANGMAN_AUTH_EMAIL_VERIFICATION_EXPIRATION_MINUTES", DefaultEmailVerificationExpirationMinutes),
                MinimumPasswordLength = GetInt("Auth.MinimumPasswordLength", "HANGMAN_AUTH_MINIMUM_PASSWORD_LENGTH", DefaultMinimumPasswordLength),
                MaximumVerificationAttempts = GetInt("Auth.MaximumVerificationAttempts", "HANGMAN_AUTH_MAXIMUM_VERIFICATION_ATTEMPTS", DefaultMaximumVerificationAttempts),
                DefaultLanguageCode = GetString("Auth.DefaultLanguageCode", "HANGMAN_AUTH_DEFAULT_LANGUAGE_CODE", DefaultLanguageCodeValue),
                AllowedLanguageCodes = GetString("Auth.AllowedLanguageCodes", "HANGMAN_AUTH_ALLOWED_LANGUAGE_CODES", DefaultAllowedLanguageCodesValue)
                    .Split(',')
                    .Select(languageCode => languageCode.Trim().ToLowerInvariant())
                    .Where(languageCode => !string.IsNullOrWhiteSpace(languageCode))
                    .ToArray()
            };
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

        private static string GetString(string appSettingKey, string environmentVariableKey, string defaultValue)
        {
            string value = GetRawValue(appSettingKey, environmentVariableKey);

            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return value.Trim();
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
