using Hangman.Business.Configuration;
using Hangman.Business.Factories;
using Hangman.Business.Helpers;
using Hangman.Business.Mappers;
using Hangman.Business.Messages;
using Hangman.Business.Policies;
using Hangman.Business.Results;
using Hangman.Business.Security;
using Hangman.Business.Validators;
using Hangman.Contracts.Auth;
using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.Auth
{
    internal class ResetPasswordUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly AuthSettingsProvider authSettingsProvider;

        public ResetPasswordUseCase(
            IUnitOfWorkFactory unitOfWorkFactory,
            AuthSettingsProvider authSettingsProvider)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.authSettingsProvider = authSettingsProvider ?? throw new ArgumentNullException(nameof(authSettingsProvider));
        }

        public async Task<ResetPasswordResponse> ExecuteAsync(ResetPasswordRequest request)
        {
            AuthSettings settings = authSettingsProvider.GetSettings();
            AuthValidator authValidator = new AuthValidator(settings);

            ValidationResult validationResult = authValidator.ValidateResetPassword(request);

            if (!validationResult.IsValid)
            {
                return AuthResponseFactory.BuildResetPasswordResponse(
                    false,
                    validationResult.MessageCode);
            }

            string email = EmailNormalizer.Normalize(request.Email);

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                AccountTransporter account = await unitOfWork.Accounts.GetByEmailAsync(email);

                if (account == null)
                {
                    return AuthResponseFactory.BuildResetPasswordResponse(
                        false,
                        AuthMessageCode.PasswordResetTokenNotFound);
                }

                if (!AccountAvailabilityPolicy.IsAvailableForPasswordReset(account))
                {
                    return AuthResponseFactory.BuildResetPasswordResponse(
                        false,
                        AuthMessageCode.AccountNotAvailable);
                }

                PasswordResetTokenTransporter token =
                    await unitOfWork.PasswordResetTokens.GetLatestUnusedByAccountIdAsync(account.AccountId);

                if (token == null)
                {
                    return AuthResponseFactory.BuildResetPasswordResponse(
                        false,
                        AuthMessageCode.PasswordResetTokenNotFound);
                }

                if (token.ExpiresAt <= DateTime.UtcNow)
                {
                    await unitOfWork.PasswordResetTokens.MarkAsUsedAsync(token.PasswordResetTokenId);
                    await unitOfWork.CommitAsync();

                    return AuthResponseFactory.BuildResetPasswordResponse(
                        false,
                        AuthMessageCode.PasswordResetTokenExpired);
                }

                if (token.Attempts >= settings.MaximumVerificationAttempts)
                {
                    return AuthResponseFactory.BuildResetPasswordResponse(
                        false,
                        AuthMessageCode.PasswordResetTokenAttemptsExceeded);
                }

                bool codeIsValid = PasswordHasher.VerifyPassword(
                    request.Code.Trim(),
                    token.ResetCodeHash);

                if (!codeIsValid)
                {
                    await unitOfWork.PasswordResetTokens.IncrementAttemptsAsync(token.PasswordResetTokenId);
                    await unitOfWork.CommitAsync();

                    return AuthResponseFactory.BuildResetPasswordResponse(
                        false,
                        AuthMessageCode.InvalidPasswordResetCode);
                }

                string newPasswordHash = PasswordHasher.HashPassword(
                    request.NewPassword,
                    settings);

                bool passwordUpdated = await unitOfWork.Accounts.UpdatePasswordHashAsync(
                    account.AccountId,
                    newPasswordHash);

                bool tokenMarkedAsUsed = await unitOfWork.PasswordResetTokens.MarkAsUsedAsync(
                    token.PasswordResetTokenId);

                if (!passwordUpdated || !tokenMarkedAsUsed)
                {
                    return AuthResponseFactory.BuildResetPasswordResponse(
                        false,
                        AuthMessageCode.PasswordResetFailed);
                }

                await unitOfWork.CommitAsync();

                return AuthResponseFactory.BuildResetPasswordResponse(
                    true,
                    AuthMessageCode.PasswordResetSuccessful);
            }
        }
    }
}
