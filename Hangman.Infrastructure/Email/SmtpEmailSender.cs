using Hangman.Business.Email;
using Hangman.Infrastructure.Email.Builders;
using Hangman.Infrastructure.Email.Configuration;
using Hangman.Infrastructure.Email.Helpers;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettingsProvider smtpSettingsProvider;
        private readonly VerificationCodeEmailBuilder verificationCodeEmailBuilder;

        public SmtpEmailSender()
        {
            smtpSettingsProvider = new SmtpSettingsProvider();
            verificationCodeEmailBuilder = new VerificationCodeEmailBuilder();
        }

        public async Task<EmailSendResult> SendVerificationCodeAsync(VerificationEmailRequest request)
        {
            if (request == null || !EmailValidation.IsValidEmail(request.RecipientEmail))
            {
                return EmailSendResult.PermanentFailure(EmailErrorCode.RecipientInvalid, null);
            }

            SmtpSettings settings = smtpSettingsProvider.GetSettings();

            if (!settings.TryValidate())
            {
                return EmailSendResult.PermanentFailure(EmailErrorCode.SmtpConfigurationMissing, null);
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
                return SmtpExceptionMapper.Map(exception);
            }
            catch (Exception exception)
            {
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
                EnableSsl = settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(settings.User, settings.Password),
                Timeout = settings.TimeoutMs
            };
        }
    }
}
