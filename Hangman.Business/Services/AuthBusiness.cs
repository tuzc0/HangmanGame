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
        private readonly PasswordHasher passwordHasher;
        private readonly VerificationCodeGenerator verificationCodeGenerator;

        public AuthBusiness(IUnitOfWorkFactory unitOfWorkFactory, IEmailSender emailSender)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));

            authSettingsProvider = new AuthSettingsProvider();
            passwordHasher = new PasswordHasher();
            verificationCodeGenerator = new VerificationCodeGenerator();
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

                string verificationCode = verificationCodeGenerator.GenerateCode(settings);
                string passwordHash = passwordHasher.HashPassword(request.Password, settings);
                string verificationCodeHash = passwordHasher.HashPassword(verificationCode, settings);

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
            AuthSettings settings = authSettingsProvider.GetSettings();
            AuthValidator authValidator = new AuthValidator(settings);

            ValidationResult validationResult = authValidator.ValidateLogin(request);

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

                bool passwordIsValid = passwordHasher.VerifyPassword(request.Password, credentials.PasswordHash);

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

        private static RegisterResponse BuildRegisterResponse(
            bool success,
            AuthMessageCode messageCode,
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
            AuthMessageCode messageCode,
            AuthenticatedPlayerDto player)
        {
            return new LoginResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Player = player
            };
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }
    }
}
