using Hangman.Business.Configuration;
using Hangman.Business.Constants;
using Hangman.Business.Email;
using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Results;
using Hangman.Business.Security;
using Hangman.Business.Validators;
using Hangman.Contracts.Auth;
using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class AuthBusiness : IAuthBusiness
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IEmailSender emailSender;
        private readonly AuthSettingsProvider authSettingsProvider;

        public AuthBusiness(IUnitOfWorkFactory unitOfWorkFactory, IEmailSender emailSender)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));

            authSettingsProvider = new AuthSettingsProvider();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                return await unitOfWork.Accounts.EmailExistsAsync(NormalizeEmail(email));
            }
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            AuthSettings settings = authSettingsProvider.GetSettings();
            AuthValidator authValidator = new AuthValidator(settings);

            ValidationResult validationResult = authValidator.ValidateRegister(request);

            if (!validationResult.IsValid)
            {
                return BuildRegisterResponse(false, validationResult.MessageCode, 0, 0, false, false);
            }

            string email = NormalizeEmail(request.Email);

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                bool emailAlreadyExists = await unitOfWork.Accounts.EmailExistsAsync(email);

                if (emailAlreadyExists)
                {
                    return BuildRegisterResponse(false, AuthMessageCode.EmailAlreadyRegistered, 0, 0, false, false);
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
                    return BuildRegisterResponse(
                        true,
                        AuthMessageCode.AccountRegisteredVerificationEmailNotSent,
                        accountId,
                        playerId,
                        true,
                        false);
                }

                return BuildRegisterResponse(
                    true,
                    AuthMessageCode.AccountRegisteredEmailVerificationRequired,
                    accountId,
                    playerId,
                    true,
                    true);
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            ValidationResult validationResult = AuthValidator.ValidateLogin(request);

            if (!validationResult.IsValid)
            {
                return BuildLoginResponse(false, validationResult.MessageCode, null);
            }

            string email = NormalizeEmail(request.Email);

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                AccountCredentialsTransporter credentials = await unitOfWork.Accounts.GetCredentialsByEmailAsync(email);

                if (credentials == null)
                {
                    return BuildLoginResponse(false, AuthMessageCode.InvalidEmailOrPassword, null);
                }

                bool passwordIsValid = PasswordHasher.VerifyPassword(request.Password, credentials.PasswordHash);

                if (!passwordIsValid)
                {
                    return BuildLoginResponse(false, AuthMessageCode.InvalidEmailOrPassword, null);
                }

                if (credentials.AccountStatus == AccountStatusConstants.Blocked ||
                    credentials.AccountStatus == AccountStatusConstants.Deleted)
                {
                    return BuildLoginResponse(false, AuthMessageCode.AccountNotAvailable, null);
                }

                if (!credentials.IsEmailVerified ||
                    credentials.AccountStatus == AccountStatusConstants.PendingVerification)
                {
                    return BuildLoginResponse(false, AuthMessageCode.EmailVerificationRequired, null);
                }

                if (credentials.AccountStatus != AccountStatusConstants.Active)
                {
                    return BuildLoginResponse(false, AuthMessageCode.AccountNotActive, null);
                }

                PlayerTransporter player = await unitOfWork.Players.GetByAccountIdAsync(credentials.AccountId);

                if (player == null || !player.IsActive)
                {
                    return BuildLoginResponse(false, AuthMessageCode.PlayerProfileNotAvailable, null);
                }

                AuthenticatedPlayerDto authenticatedPlayer = new AuthenticatedPlayerDto
                {
                    AccountId = credentials.AccountId,
                    PlayerId = player.PlayerId,
                    FullName = player.FullName,
                    DateOfBirth = player.DateOfBirth,
                    Phone = player.Phone,
                    Email = credentials.Email,
                    PreferredLanguageCode = player.PreferredLanguageCode
                };

                return BuildLoginResponse(true, AuthMessageCode.LoginSuccessful, authenticatedPlayer);
            }
        }

        public async Task<ResendVerificationEmailResponse> ResendVerificationEmailAsync(
            ResendVerificationEmailRequest request)
        {
            ValidationResult validationResult = AuthValidator.ValidateResendVerificationEmail(request);

            if (!validationResult.IsValid)
            {
                return BuildResendVerificationEmailResponse(
                    false,
                    validationResult.MessageCode,
                    false);
            }

            AuthSettings settings = authSettingsProvider.GetSettings();
            string email = NormalizeEmail(request.Email);

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                AccountTransporter account = await unitOfWork.Accounts.GetByEmailAsync(email);

                if (account == null)
                {
                    return BuildResendVerificationEmailResponse(
                        true,
                        AuthMessageCode.VerificationEmailResendProcessed,
                        false);
                }

                if (account.AccountStatus == AccountStatusConstants.Blocked ||
                    account.AccountStatus == AccountStatusConstants.Deleted)
                {
                    return BuildResendVerificationEmailResponse(
                        false,
                        AuthMessageCode.AccountNotAvailable,
                        false);
                }

                if (account.IsEmailVerified ||
                    account.AccountStatus == AccountStatusConstants.Active)
                {
                    return BuildResendVerificationEmailResponse(
                        true,
                        AuthMessageCode.AccountAlreadyVerified,
                        false);
                }

                PlayerTransporter player = await unitOfWork.Players.GetByIdAsync(account.PlayerId);

                if (player == null || !player.IsActive)
                {
                    return BuildResendVerificationEmailResponse(
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
                    return BuildResendVerificationEmailResponse(
                        true,
                        AuthMessageCode.VerificationEmailResendFailed,
                        false);
                }

                return BuildResendVerificationEmailResponse(
                    true,
                    AuthMessageCode.VerificationEmailResent,
                    true);
            }
        }

        private static RegisterResponse BuildRegisterResponse(
            bool success,
            Enum messageCode,
            int accountId,
            int playerId,
            bool requiresEmailVerification,
            bool verificationEmailSent)
        {
            return new RegisterResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                AccountId = accountId,
                PlayerId = playerId,
                RequiresEmailVerification = requiresEmailVerification,
                VerificationEmailSent = verificationEmailSent
            };
        }

        private static LoginResponse BuildLoginResponse(
            bool success,
            Enum messageCode,
            AuthenticatedPlayerDto player)
        {
            return new LoginResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Player = player
            };
        }

        private static ResendVerificationEmailResponse BuildResendVerificationEmailResponse(
            bool success,
            Enum messageCode,
            bool verificationEmailSent)
        {
            return new ResendVerificationEmailResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                VerificationEmailSent = verificationEmailSent
            };
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }
    }
}
