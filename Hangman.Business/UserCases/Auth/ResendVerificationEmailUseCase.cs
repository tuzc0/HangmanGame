using Hangman.Business.Configuration;
using Hangman.Business.Email;
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
    internal class ResendVerificationEmailUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IEmailSender emailSender;
        private readonly AuthSettingsProvider authSettingsProvider;

        public ResendVerificationEmailUseCase(
            IUnitOfWorkFactory unitOfWorkFactory,
            IEmailSender emailSender,
            AuthSettingsProvider authSettingsProvider)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            this.authSettingsProvider = authSettingsProvider ?? throw new ArgumentNullException(nameof(authSettingsProvider));
        }

        public async Task<ResendVerificationEmailResponse> ExecuteAsync(
            ResendVerificationEmailRequest request)
        {
            ValidationResult validationResult = AuthValidator.ValidateResendVerificationEmail(request);

            if (!validationResult.IsValid)
            {
                return AuthResponseFactory.BuildResendVerificationEmailResponse(
                    false,
                    validationResult.MessageCode,
                    false);
            }

            AuthSettings settings = authSettingsProvider.GetSettings();
            string email = EmailNormalizer.Normalize(request.Email);

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                AccountTransporter account = await unitOfWork.Accounts.GetByEmailAsync(email);

                if (account == null)
                {
                    return AuthResponseFactory.BuildResendVerificationEmailResponse(
                        true,
                        AuthMessageCode.VerificationEmailResendProcessed,
                        false);
                }

                if (account.AccountStatus == Constants.AccountStatusConstants.Blocked ||
                    account.AccountStatus == Constants.AccountStatusConstants.Deleted)
                {
                    return AuthResponseFactory.BuildResendVerificationEmailResponse(
                        false,
                        AuthMessageCode.AccountNotAvailable,
                        false);
                }

                if (account.IsEmailVerified ||
                    account.AccountStatus == Constants.AccountStatusConstants.Active)
                {
                    return AuthResponseFactory.BuildResendVerificationEmailResponse(
                        true,
                        AuthMessageCode.AccountAlreadyVerified,
                        false);
                }

                PlayerTransporter player = await unitOfWork.Players.GetByIdAsync(account.PlayerId);

                if (player == null || !player.IsActive)
                {
                    return AuthResponseFactory.BuildResendVerificationEmailResponse(
                        false,
                        AuthMessageCode.PlayerProfileNotAvailable,
                        false);
                }

                string verificationCode = VerificationCodeGenerator.GenerateCode(settings);
                string verificationCodeHash = PasswordHasher.HashPassword(verificationCode, settings);

                await unitOfWork.EmailVerifications.InvalidateUnusedByAccountIdAsync(account.AccountId);

                unitOfWork.EmailVerifications.Add(
                    new CreateEmailVerificationTransporter
                    {
                        AccountId = account.AccountId,
                        VerificationCodeHash = verificationCodeHash,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(settings.EmailVerificationExpirationMinutes)
                    });

                await unitOfWork.CommitAsync();

                EmailSendResult emailSendResult = await emailSender.SendVerificationCodeAsync(
                    new VerificationEmailRequest
                    {
                        RecipientEmail = account.Email,
                        RecipientName = player.FullName,
                        VerificationCode = verificationCode,
                        ExpirationMinutes = settings.EmailVerificationExpirationMinutes,
                        LanguageCode = player.PreferredLanguageCode
                    });

                if (!emailSendResult.IsSuccess)
                {
                    return AuthResponseFactory.BuildResendVerificationEmailResponse(
                        true,
                        AuthMessageCode.VerificationEmailResendFailed,
                        false);
                }

                return AuthResponseFactory.BuildResendVerificationEmailResponse(
                    true,
                    AuthMessageCode.VerificationEmailResent,
                    true);
            }
        }
    }
}
