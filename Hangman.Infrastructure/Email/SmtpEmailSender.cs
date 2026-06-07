using Hangman.Business.Email;
using Hangman.Infrastructure.Email.Builders;
using Hangman.Infrastructure.Email.Configuration;
using Hangman.Infrastructure.Email.Helpers;
using log4net;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SmtpEmailSender));

        private readonly SmtpSettingsProvider smtpSettingsProvider;
        private readonly VerificationCodeEmailBuilder verificationCodeEmailBuilder;
        private readonly PasswordResetEmailBuilder passwordResetEmailBuilder;

        public SmtpEmailSender()
        {
            smtpSettingsProvider = new SmtpSettingsProvider();
            verificationCodeEmailBuilder = new VerificationCodeEmailBuilder();
            passwordResetEmailBuilder = new PasswordResetEmailBuilder();
        }

        public async Task<EmailSendResult> SendVerificationCodeAsync(VerificationEmailRequest request)
        {
            if (request == null || !EmailValidation.IsValidEmail(request.RecipientEmail))
            {
                Log.Warn("Verification email was not sent. Reason: recipient email is invalid.");

                return EmailSendResult.PermanentFailure(EmailErrorCode.RecipientInvalid, null);
            }

            VerificationCodeEmailContext context = new VerificationCodeEmailContext
            {
                RecipientEmail = request.RecipientEmail,
                RecipientName = request.RecipientName,
                VerificationCode = request.VerificationCode,
                ExpirationMinutes = request.ExpirationMinutes,
                LanguageCode = request.LanguageCode
            };

            EmailMessage emailMessage = verificationCodeEmailBuilder.Build(context);

            return await SendEmailMessageAsync(emailMessage, "Verification", request.RecipientEmail);
        }

        public async Task<EmailSendResult> SendPasswordResetCodeAsync(PasswordResetEmailRequest request)
        {
            if (request == null || !EmailValidation.IsValidEmail(request.RecipientEmail))
            {
                Log.Warn("Password reset email was not sent. Reason: recipient email is invalid.");

                return EmailSendResult.PermanentFailure(EmailErrorCode.RecipientInvalid, null);
            }

            PasswordResetEmailContext context = new PasswordResetEmailContext
            {
                RecipientEmail = request.RecipientEmail,
                RecipientName = request.RecipientName,
                ResetCode = request.ResetCode,
                ExpirationMinutes = request.ExpirationMinutes,
                LanguageCode = request.LanguageCode
            };

            EmailMessage emailMessage = passwordResetEmailBuilder.Build(context);

            return await SendEmailMessageAsync(emailMessage, "PasswordReset", request.RecipientEmail);
        }

        private async Task<EmailSendResult> SendEmailMessageAsync(
            EmailMessage emailMessage,
            string emailType,
            string recipientEmail)
        {
            SmtpSettings settings = smtpSettingsProvider.GetSettings();

            if (!settings.TryValidate())
            {
                Log.ErrorFormat("{0} email was not sent. Reason: SMTP configuration is missing or invalid.",
                    emailType);

                return EmailSendResult.PermanentFailure(EmailErrorCode.SmtpConfigurationMissing, null);
            }

            try
            {
                using (MailMessage mailMessage = CreateMailMessage(emailMessage, settings))
                using (SmtpClient smtpClient = CreateClient(settings))
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }

                return EmailSendResult.Success();
            }
            catch (SmtpException exception)
            {
                EmailSendResult result = SmtpExceptionMapper.Map(exception);

                LogSmtpFailure(result, recipientEmail, emailType, exception);

                return result;
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("{0} email failed with unexpected error. Recipient: {1}",
                    emailType,
                    MaskEmail(recipientEmail),
                    exception);

                return EmailSendResult.PermanentFailure(EmailErrorCode.SmtpUnknown, exception);
            }
        }

        private static MailMessage CreateMailMessage(EmailMessage emailMessage, SmtpSettings settings)
        {
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(settings.FromAddress, settings.DisplayName, Encoding.UTF8),
                Subject = emailMessage.Subject,
                Body = emailMessage.Body,
                IsBodyHtml = emailMessage.IsBodyHtml,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                HeadersEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(emailMessage.Recipient);

            return mailMessage;
        }

        private static SmtpClient CreateClient(SmtpSettings settings)
        {
            return new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(settings.User, settings.Password),
                Timeout = settings.TimeoutMs
            };
        }

        private static void LogSmtpFailure(
            EmailSendResult result,
            string recipientEmail,
            string emailType,
            SmtpException exception)
        {
            string logMessage = string.Format(
                "{0} email failed. Status: {1}. ErrorCode: {2}. Recipient: {3}. SmtpStatusCode: {4}",
                emailType,
                result.Status,
                result.ErrorCode,
                MaskEmail(recipientEmail),
                exception.StatusCode);

            if (result.Status == EmailSendStatus.TemporaryFailure)
            {
                Log.Warn(logMessage, exception);
                return;
            }

            Log.Error(logMessage, exception);
        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                return "invalid-email";
            }

            string[] parts = email.Split('@');

            if (parts[0].Length <= 2)
            {
                return "***@" + parts[1];
            }

            return parts[0].Substring(0, 2) + "***@" + parts[1];
        }
    }
}
