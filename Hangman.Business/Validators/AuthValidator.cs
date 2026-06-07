using Hangman.Business.Configuration;
using Hangman.Business.Messages;
using Hangman.Business.Results;
using Hangman.Contracts.Auth;
using System;
using System.Linq;
using System.Net.Mail;

namespace Hangman.Business.Validators
{
    public class AuthValidator
    {
        private readonly AuthSettings settings;

        public AuthValidator(AuthSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public ValidationResult ValidateRegister(RegisterRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(AuthMessageCode.RegistrationDataRequired);
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return ValidationResult.Fail(AuthMessageCode.FullNameRequired);
            }

            if (request.DateOfBirth == default(DateTime) || request.DateOfBirth >= DateTime.Today)
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidDateOfBirth);
            }

            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                return ValidationResult.Fail(AuthMessageCode.PhoneRequired);
            }

            if (!IsValidLanguageCode(request.PreferredLanguageCode))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidPreferredLanguage);
            }

            if (!IsValidEmail(request.Email))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidEmail);
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordRequired);
            }

            if (request.Password.Length < settings.MinimumPasswordLength)
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordTooShort);
            }

            return ValidationResult.Success();
        }

        public static ValidationResult ValidateLogin(LoginRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(AuthMessageCode.LoginDataRequired);
            }

            if (!IsValidEmail(request.Email))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidEmail);
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordRequired);
            }

            return ValidationResult.Success();
        }

        public static ValidationResult ValidateResendVerificationEmail(ResendVerificationEmailRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(AuthMessageCode.RegistrationDataRequired);
            }

            if (!IsValidEmail(request.Email))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidEmail);
            }

            return ValidationResult.Success();
        }

        private bool IsValidLanguageCode(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return false;
            }

            string normalizedLanguageCode = languageCode.Trim().ToLowerInvariant();

            return settings.AllowedLanguageCodes != null &&
                   settings.AllowedLanguageCodes.Contains(normalizedLanguageCode);
        }

        public static ValidationResult ValidateRequestPasswordReset(RequestPasswordResetRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordResetRequestDataRequired);
            }

            if (!IsValidEmail(request.Email))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidEmail);
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateResetPassword(ResetPasswordRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordResetDataRequired);
            }

            if (!IsValidEmail(request.Email))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidEmail);
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidPasswordResetCode);
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordRequired);
            }

            if (request.NewPassword.Length < settings.MinimumPasswordLength)
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordTooShort);
            }

            return ValidationResult.Success();
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                MailAddress mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
