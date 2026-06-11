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

            ValidationResult fullNameValidation = ValidateFullName(request.FullName);

            if (!fullNameValidation.IsValid)
            {
                return fullNameValidation;
            }

            ValidationResult dateValidation = ValidateDateOfBirth(request.DateOfBirth);

            if (!dateValidation.IsValid)
            {
                return dateValidation;
            }

            ValidationResult phoneValidation = ValidatePhone(request.Phone);

            if (!phoneValidation.IsValid)
            {
                return phoneValidation;
            }

            if (!IsValidLanguageCode(request.PreferredLanguageCode))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidPreferredLanguage);
            }

            ValidationResult emailValidation = ValidateEmail(request.Email);

            if (!emailValidation.IsValid)
            {
                return emailValidation;
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

            ValidationResult emailValidation = ValidateEmail(request.Email);

            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            ValidationResult codeValidation = ValidateNumericCode(
                request.Code,
                settings.VerificationCodeLength,
                AuthMessageCode.VerificationCodeRequired,
                AuthMessageCode.InvalidEmailVerificationCode);

            if (!codeValidation.IsValid)
            {
                return codeValidation;
            }

            return ValidationResult.Success();
        }

        public static ValidationResult ValidateLogin(LoginRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(AuthMessageCode.LoginDataRequired);
            }

            ValidationResult emailValidation = ValidateEmail(request.Email);

            if (!emailValidation.IsValid)
            {
                return emailValidation;
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

            ValidationResult emailValidation = ValidateEmail(request.Email);

            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            return ValidationResult.Success();
        }

        public static ValidationResult ValidateRequestPasswordReset(RequestPasswordResetRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordResetRequestDataRequired);
            }

            ValidationResult emailValidation = ValidateEmail(request.Email);

            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateResetPassword(ResetPasswordRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(AuthMessageCode.PasswordResetDataRequired);
            }

            ValidationResult emailValidation = ValidateEmail(request.Email);

            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            ValidationResult codeValidation = ValidateNumericCode(
                request.Code,
                settings.VerificationCodeLength,
                AuthMessageCode.InvalidPasswordResetCode,
                AuthMessageCode.InvalidPasswordResetCode);

            if (!codeValidation.IsValid)
            {
                return codeValidation;
            }

            ValidationResult passwordValidation = ValidatePasswordPolicy(request.NewPassword);

            if (!passwordValidation.IsValid)
            {
                return passwordValidation;
            }

            return ValidationResult.Success();
        }

        private static ValidationResult ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return ValidationResult.Fail(AuthMessageCode.FullNameRequired);
            }

            string normalizedFullName = fullName.Trim();

            if (normalizedFullName.Length < ValidationLimits.FullNameMinimumLength)
            {
                return ValidationResult.Fail(AuthMessageCode.FullNameTooShort);
            }

            if (normalizedFullName.Length > ValidationLimits.FullNameMaximumLength)
            {
                return ValidationResult.Fail(AuthMessageCode.FullNameTooLong);
            }

            return ValidationResult.Success();
        }

        private static ValidationResult ValidateDateOfBirth(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Today;
            DateTime minimumAllowedDate = today.AddYears(-ValidationLimits.MaximumAgeInYears);

            if (dateOfBirth == default(DateTime) ||
                dateOfBirth.Date >= today ||
                dateOfBirth.Date < minimumAllowedDate)
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidDateOfBirth);
            }

            return ValidationResult.Success();
        }

        private static ValidationResult ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return ValidationResult.Fail(AuthMessageCode.PhoneRequired);
            }

            string normalizedPhone = phone.Trim();

            if (normalizedPhone.Length < ValidationLimits.PhoneMinimumLength)
            {
                return ValidationResult.Fail(AuthMessageCode.PhoneTooShort);
            }

            if (normalizedPhone.Length > ValidationLimits.PhoneMaximumLength)
            {
                return ValidationResult.Fail(AuthMessageCode.PhoneTooLong);
            }

            if (!normalizedPhone.All(char.IsDigit))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidPhone);
            }

            return ValidationResult.Success();
        }

        private static ValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidEmail);
            }

            string normalizedEmail = email.Trim();

            if (normalizedEmail.Length > ValidationLimits.EmailMaximumLength)
            {
                return ValidationResult.Fail(AuthMessageCode.EmailTooLong);
            }

            try
            {
                MailAddress mailAddress = new MailAddress(normalizedEmail);

                if (!string.Equals(mailAddress.Address, normalizedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return ValidationResult.Fail(AuthMessageCode.InvalidEmail);
                }

                return ValidationResult.Success();
            }
            catch (FormatException)
            {
                return ValidationResult.Fail(AuthMessageCode.InvalidEmail);
            }
        }

        private static ValidationResult ValidateNumericCode(
            string code,
            int requiredLength,
            AuthMessageCode requiredCode,
            AuthMessageCode invalidCode)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ValidationResult.Fail(requiredCode);
            }

            string normalizedCode = code.Trim();

            if (normalizedCode.Length != requiredLength)
            {
                return ValidationResult.Fail(invalidCode);
            }

            if (!normalizedCode.All(char.IsDigit))
            {
                return ValidationResult.Fail(invalidCode);
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
            return value.Any(char.IsWhiteSpace);
        }

        private static bool IsSpecialCharacter(char character)
        {
            return !char.IsLetterOrDigit(character) && !char.IsWhiteSpace(character);
        }
    }
}
