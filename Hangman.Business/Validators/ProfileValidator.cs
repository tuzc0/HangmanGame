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

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return ValidationResult.Fail(ProfileMessageCode.FullNameRequired);
            }

            if (request.DateOfBirth == default(DateTime) || request.DateOfBirth >= DateTime.Today)
            {
                return ValidationResult.Fail(ProfileMessageCode.InvalidDateOfBirth);
            }

            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                return ValidationResult.Fail(ProfileMessageCode.PhoneRequired);
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
