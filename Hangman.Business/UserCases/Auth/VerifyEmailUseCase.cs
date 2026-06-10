using Hangman.Business.Configuration;
using Hangman.Business.Constants;
using Hangman.Business.Factories;
using Hangman.Business.Helpers;
using Hangman.Business.Mappers;
using Hangman.Business.Messages;
using Hangman.Business.Results;
using Hangman.Business.Security;
using Hangman.Business.Validators;
using Hangman.Contracts.Auth;
using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.Auth
{
    internal class VerifyEmailUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly AuthSettingsProvider authSettingsProvider;

        public VerifyEmailUseCase(
            IUnitOfWorkFactory unitOfWorkFactory,
            AuthSettingsProvider authSettingsProvider)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? 
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.authSettingsProvider = authSettingsProvider ?? 
                throw new ArgumentNullException(nameof(authSettingsProvider));
        }

        public async Task<VerifyEmailResponse> ExecuteAsync(VerifyEmailRequest request)
        {
            AuthSettings settings = authSettingsProvider.GetSettings();
            AuthValidator authValidator = new AuthValidator(settings);

            ValidationResult validationResult = authValidator.ValidateVerifyEmail(request);

            if (!validationResult.IsValid)
            {
                return AuthResponseFactory.BuildVerifyEmailResponse(
                    false,
                    validationResult.MessageCode,
                    0,
                    false);
            }

            string email = EmailNormalizer.Normalize(request.Email);

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                AccountTransporter account = await unitOfWork.Accounts.GetByEmailAsync(email);

                if (account == null)
                {
                    return AuthResponseFactory.BuildVerifyEmailResponse(
                        false,
                        AuthMessageCode.EmailVerificationTokenNotFound,
                        0,
                        false);
                }

                if (account.AccountStatus == AccountStatusConstants.Blocked ||
                    account.AccountStatus == AccountStatusConstants.Deleted)
                {
                    return AuthResponseFactory.BuildVerifyEmailResponse(
                        false,
                        AuthMessageCode.AccountNotAvailable,
                        account.AccountId,
                        false);
                }

                if (account.IsEmailVerified ||
                    account.AccountStatus == AccountStatusConstants.Active)
                {
                    return AuthResponseFactory.BuildVerifyEmailResponse(
                        true,
                        AuthMessageCode.AccountAlreadyVerified,
                        account.AccountId,
                        true);
                }

                EmailVerificationTokenTransporter token =
                    await unitOfWork.EmailVerifications.GetLatestUnusedByAccountIdAsync(account.AccountId);

                if (token == null)
                {
                    return AuthResponseFactory.BuildVerifyEmailResponse(
                        false,
                        AuthMessageCode.EmailVerificationTokenNotFound,
                        account.AccountId,
                        false);
                }

                if (token.ExpiresAt <= DateTime.UtcNow)
                {
                    await unitOfWork.EmailVerifications.MarkAsUsedAsync(token.EmailVerificationId);
                    await unitOfWork.CommitAsync();

                    return AuthResponseFactory.BuildVerifyEmailResponse(
                        false,
                        AuthMessageCode.EmailVerificationTokenExpired,
                        account.AccountId,
                        false);
                }

                if (token.Attempts >= settings.MaximumVerificationAttempts)
                {
                    return AuthResponseFactory.BuildVerifyEmailResponse(
                        false,
                        AuthMessageCode.EmailVerificationTokenAttemptsExceeded,
                        account.AccountId,
                        false);
                }

                bool codeIsValid = PasswordHasher.VerifyPassword(
                    request.Code.Trim(),
                    token.VerificationCodeHash);

                if (!codeIsValid)
                {
                    await unitOfWork.EmailVerifications.IncrementAttemptsAsync(token.EmailVerificationId);
                    await unitOfWork.CommitAsync();

                    return AuthResponseFactory.BuildVerifyEmailResponse(
                        false,
                        AuthMessageCode.InvalidEmailVerificationCode,
                        account.AccountId,
                        false);
                }

                bool accountMarkedAsVerified = await unitOfWork.Accounts.MarkEmailAsVerifiedAsync(
                    new MarkEmailAsVerifiedTransporter
                    {
                        AccountId = account.AccountId,
                        AccountStatus = AccountStatusConstants.Active
                    });

                bool tokenMarkedAsUsed = await unitOfWork.EmailVerifications.MarkAsUsedAsync(
                    token.EmailVerificationId);

                if (!accountMarkedAsVerified || !tokenMarkedAsUsed)
                {
                    return AuthResponseFactory.BuildVerifyEmailResponse(
                        false,
                        AuthMessageCode.EmailVerificationFailed,
                        account.AccountId,
                        false);
                }

                await unitOfWork.CommitAsync();

                return AuthResponseFactory.BuildVerifyEmailResponse(
                    true,
                    AuthMessageCode.EmailVerificationSuccessful,
                    account.AccountId,
                    true);
            }
        }
    }
}
