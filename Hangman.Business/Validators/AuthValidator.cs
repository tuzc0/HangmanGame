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

            ValidationResult passwordValidation = ValidatePasswordPolicy(request.Password);

            if (!passwordValidation.IsValid)
            {
                return passwordValidation;
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateVerifyEmail(VerifyEmailRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(AuthMessageCode.EmailVerificationDataRequired);
            }

            if (!IsValidEmail(request.Email))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidEmail);
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return ValidationResult.Fail(AuthMessageCode.VerificationCodeRequired);
            }

            string code = request.Code.Trim();

            if (code.Length != settings.VerificationCodeLength)
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidEmailVerificationCode);
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

            ValidationResult passwordValidation = ValidatePasswordPolicy(request.NewPassword);

            if (!passwordValidation.IsValid)
            {
                return passwordValidation;
            }

            return ValidationResult.Success();
        }

        private ValidationResult ValidatePasswordPolicy(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordRequired);
            }

            if (password.Length < settings.MinimumPasswordLength)
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordTooShort);
            }

            if (password.Length > settings.MaximumPasswordLength)
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordTooLong);
            }

            if (settings.DisallowPasswordWhiteSpace && ContainsWhiteSpace(password))
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordContainsWhiteSpace);
            }

            if (settings.RequirePasswordUppercase && !password.Any(char.IsUpper))
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordRequiresUppercase);
            }

            if (settings.RequirePasswordLowercase && !password.Any(char.IsLower))
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordRequiresLowercase);
            }

            if (settings.RequirePasswordDigit && !password.Any(char.IsDigit))
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordRequiresDigit);
            }

            if (settings.RequirePasswordSpecialCharacter && !password.Any(IsSpecialCharacter))
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordRequiresSpecialCharacter);
            }

            return ValidationResult.Success();
        }

        private static bool ContainsWhiteSpace(string value)
        {
            foreach (char character in value)
            {
                if (char.IsWhiteSpace(character))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSpecialCharacter(char character)
        {
            return !char.IsLetterOrDigit(character) && !char.IsWhiteSpace(character);
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
