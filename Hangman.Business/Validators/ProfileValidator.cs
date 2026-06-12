using Hangman.Business.Configuration;
using Hangman.Business.Messages;
using Hangman.Business.Results;
using Hangman.Contracts.Profile;
using System;
using System.Linq;

namespace Hangman.Business.Validators
{
    public class ProfileValidator
    {
        private readonly AuthSettings settings;

        public ProfileValidator(AuthSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public static ValidationResult ValidateGetProfile(GetProfileRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(ProfileMessageCode.ProfileDataRequired);
            }

            if (request.AccountId <= 0)
            {
                return ValidationResult.Fail(ProfileMessageCode.InvalidAccountId);
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateUpdateProfile(UpdateProfileRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(ProfileMessageCode.ProfileDataRequired);
            }

            if (request.AccountId <= 0)
            {
                return ValidationResult.Fail(ProfileMessageCode.InvalidAccountId);
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
                return ValidationResult.Fail(ProfileMessageCode.InvalidPreferredLanguage);
            }

            return ValidationResult.Success();
        }

        public static ValidationResult ValidateDeleteProfile(DeleteProfileRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(ProfileMessageCode.ProfileDataRequired);
            }

            if (request.AccountId <= 0)
            {
                return ValidationResult.Fail(ProfileMessageCode.InvalidAccountId);
            }

            return ValidationResult.Success();
        }

        private static ValidationResult ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return ValidationResult.Fail(ProfileMessageCode.FullNameRequired);
            }

            string normalizedFullName = fullName.Trim();

            if (normalizedFullName.Length < ValidationLimits.FullNameMinimumLength)
            {
                return ValidationResult.Fail(ProfileMessageCode.FullNameTooShort);
            }

            if (normalizedFullName.Length > ValidationLimits.FullNameMaximumLength)
            {
                return ValidationResult.Fail(ProfileMessageCode.FullNameTooLong);
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
                return ValidationResult.Fail(ProfileMessageCode.InvalidDateOfBirth);
            }

            return ValidationResult.Success();
        }

        private static ValidationResult ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return ValidationResult.Fail(ProfileMessageCode.PhoneRequired);
            }

            string normalizedPhone = phone.Trim();

            if (normalizedPhone.Length < ValidationLimits.PhoneMinimumLength)
            {
                return ValidationResult.Fail(ProfileMessageCode.PhoneTooShort);
            }

            if (normalizedPhone.Length > ValidationLimits.PhoneMaximumLength)
            {
                return ValidationResult.Fail(ProfileMessageCode.PhoneTooLong);
            }

            if (!normalizedPhone.All(char.IsDigit))
            {
                return ValidationResult.Fail(ProfileMessageCode.InvalidPhone);
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
    }
}
