using Hangman.Business.Configuration;
using Hangman.Business.Email;
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
    internal class RequestPasswordResetUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IEmailSender emailSender;
        private readonly AuthSettingsProvider authSettingsProvider;

        public RequestPasswordResetUseCase(
            IUnitOfWorkFactory unitOfWorkFactory,
            IEmailSender emailSender,
            AuthSettingsProvider authSettingsProvider)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            this.authSettingsProvider = authSettingsProvider ?? throw new ArgumentNullException(nameof(authSettingsProvider));
        }

        public async Task<RequestPasswordResetResponse> ExecuteAsync(
            RequestPasswordResetRequest request)
        {
            ValidationResult validationResult = AuthValidator.ValidateRequestPasswordReset(request);

            if (!validationResult.IsValid)
            {
                return AuthResponseFactory.BuildRequestPasswordResetResponse(
                    false,
                    validationResult.MessageCode);
            }

            AuthSettings settings = authSettingsProvider.GetSettings();
            string email = EmailNormalizer.Normalize(request.Email);

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                AccountTransporter account = await unitOfWork.Accounts.GetByEmailAsync(email);

                if (account == null)
                {
                    return AuthResponseFactory.BuildRequestPasswordResetResponse(
                        true,
                        AuthMessageCode.PasswordResetRequestProcessed);
                }

                if (!AccountAvailabilityPolicy.IsAvailableForPasswordReset(account))
                {
                    return AuthResponseFactory.BuildRequestPasswordResetResponse(
                        true,
                        AuthMessageCode.PasswordResetRequestProcessed);
                }

                PlayerTransporter player = await unitOfWork.Players.GetByIdAsync(account.PlayerId);

                if (player == null || !player.IsActive)
                {
                    return AuthResponseFactory.BuildRequestPasswordResetResponse(
                        true,
                        AuthMessageCode.PasswordResetRequestProcessed);
                }

                string resetCode = VerificationCodeGenerator.GenerateCode(settings);
                string resetCodeHash = PasswordHasher.HashPassword(resetCode, settings);

                await unitOfWork.PasswordResetTokens.InvalidateUnusedByAccountIdAsync(account.AccountId);

                unitOfWork.PasswordResetTokens.Add(
                    new CreatePasswordResetTokenTransporter
                    {
                        AccountId = account.AccountId,
                        ResetCodeHash = resetCodeHash,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(settings.PasswordResetExpirationMinutes)
                    });

                await unitOfWork.CommitAsync();

                EmailSendResult emailSendResult = await emailSender.SendPasswordResetCodeAsync(
                    new PasswordResetEmailRequest
                    {
                        RecipientEmail = account.Email,
                        RecipientName = player.FullName,
                        ResetCode = resetCode,
                        ExpirationMinutes = settings.PasswordResetExpirationMinutes,
                        LanguageCode = player.PreferredLanguageCode
                    });

                if (!emailSendResult.IsSuccess)
                {
                    return AuthResponseFactory.BuildRequestPasswordResetResponse(
                        true,
                        AuthMessageCode.PasswordResetEmailFailed);
                }

                return AuthResponseFactory.BuildRequestPasswordResetResponse(
                    true,
                    AuthMessageCode.PasswordResetEmailSent);
            }
        }
    }
}
