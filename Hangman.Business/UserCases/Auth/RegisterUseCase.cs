using Hangman.Business.Configuration;
using Hangman.Business.Constants;
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
    internal class RegisterUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IEmailSender emailSender;
        private readonly AuthSettingsProvider authSettingsProvider;

        public RegisterUseCase(
            IUnitOfWorkFactory unitOfWorkFactory,
            IEmailSender emailSender,
            AuthSettingsProvider authSettingsProvider)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            this.authSettingsProvider = authSettingsProvider ?? throw new ArgumentNullException(nameof(authSettingsProvider));
        }

        public async Task<RegisterResponse> ExecuteAsync(RegisterRequest request)
        {
            AuthSettings settings = authSettingsProvider.GetSettings();
            AuthValidator authValidator = new AuthValidator(settings);

            ValidationResult validationResult = authValidator.ValidateRegister(request);

            if (!validationResult.IsValid)
            {
                return AuthResponseFactory.BuildRegisterResponse(
                    false,
                    validationResult.MessageCode,
                    0,
                    0,
                    false,
                    false);
            }

            string email = EmailNormalizer.Normalize(request.Email);

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                bool emailAlreadyExists = await unitOfWork.Accounts.EmailExistsAsync(email);

                if (emailAlreadyExists)
                {
                    return AuthResponseFactory.BuildRegisterResponse(
                        false,
                        AuthMessageCode.EmailAlreadyRegistered,
                        0,
                        0,
                        false,
                        false);
                }

                string verificationCode = VerificationCodeGenerator.GenerateCode(settings);
                string passwordHash = PasswordHasher.HashPassword(request.Password, settings);
                string verificationCodeHash = PasswordHasher.HashPassword(verificationCode, settings);

                CreatePendingAccountTransporter registration = new CreatePendingAccountTransporter
                {
                    FullName = request.FullName.Trim(),
                    DateOfBirth = request.DateOfBirth,
                    Phone = request.Phone.Trim(),
                    IsPlayerActive = true,
                    PreferredLanguageCode = request.PreferredLanguageCode.Trim().ToLowerInvariant(),

                    Email = email,
                    PasswordHash = passwordHash,
                    IsEmailVerified = false,
                    EmailVerifiedAt = null,
                    AccountStatus = AccountStatusConstants.PendingVerification,

                    VerificationCodeHash = verificationCodeHash,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(settings.EmailVerificationExpirationMinutes)
                };

                unitOfWork.Accounts.AddPendingAccount(registration);
                await unitOfWork.CommitAsync();

                AccountTransporter createdAccount = await unitOfWork.Accounts.GetByEmailAsync(email);

                int accountId = createdAccount != null ? createdAccount.AccountId : 0;
                int playerId = createdAccount != null ? createdAccount.PlayerId : 0;

                EmailSendResult emailSendResult = await emailSender.SendVerificationCodeAsync(
                    new VerificationEmailRequest
                    {
                        RecipientEmail = email,
                        RecipientName = request.FullName.Trim(),
                        VerificationCode = verificationCode,
                        ExpirationMinutes = settings.EmailVerificationExpirationMinutes,
                        LanguageCode = request.PreferredLanguageCode.Trim().ToLowerInvariant()
                    });

                if (!emailSendResult.IsSuccess)
                {
                    return AuthResponseFactory.BuildRegisterResponse(
                        true,
                        AuthMessageCode.AccountRegisteredVerificationEmailNotSent,
                        accountId,
                        playerId,
                        true,
                        false);
                }

                return AuthResponseFactory.BuildRegisterResponse(
                    true,
                    AuthMessageCode.AccountRegisteredEmailVerificationRequired,
                    accountId,
                    playerId,
                    true,
                    true);
            }
        }
    }
}
