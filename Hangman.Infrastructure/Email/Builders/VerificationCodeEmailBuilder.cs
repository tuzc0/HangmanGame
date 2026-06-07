using System;

namespace Hangman.Infrastructure.Email.Builders
{
    internal class VerificationCodeEmailBuilder : IEmailMessageBuilder<VerificationCodeEmailContext>
    {
        private const string SpanishLanguageCode = "es";
        private const bool IsBodyHtml = false;

        public EmailMessage Build(VerificationCodeEmailContext context)
        {
            if (context == null)
            {
                return new EmailMessage(string.Empty, string.Empty, string.Empty, false);
            }

            string languageCode = NormalizeLanguageCode(context.LanguageCode);

            if (languageCode == SpanishLanguageCode)
            {
                return BuildSpanishMessage(context);
            }

            return BuildEnglishMessage(context);
        }

        private static EmailMessage BuildSpanishMessage(VerificationCodeEmailContext context)
        {
            string subject = "Código de verificación - Hangman Game";

            string body = string.Format(
                "Hola {0},{1}{1}Tu código de verificación es: {2}{1}{1}Este código expira en {3} minutos.{1}{1}Si no solicitaste esta cuenta, puedes ignorar este correo.",
                GetRecipientName(context.RecipientName),
                Environment.NewLine,
                context.VerificationCode,
                context.ExpirationMinutes);

            return new EmailMessage(context.RecipientEmail, subject, body, IsBodyHtml);
        }

        private static EmailMessage BuildEnglishMessage(VerificationCodeEmailContext context)
        {
            string subject = "Verification code - Hangman Game";

            string body = string.Format(
                "Hello {0},{1}{1}Your verification code is: {2}{1}{1}This code expires in {3} minutes.{1}{1}If you did not request this account, you can ignore this email.",
                GetRecipientName(context.RecipientName),
                Environment.NewLine,
                context.VerificationCode,
                context.ExpirationMinutes);

            return new EmailMessage(context.RecipientEmail, subject, body, IsBodyHtml);
        }

        private static string GetRecipientName(string recipientName)
        {
            if (string.IsNullOrWhiteSpace(recipientName))
            {
                return "Player";
            }

            return recipientName.Trim();
        }

        private static string NormalizeLanguageCode(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return SpanishLanguageCode;
            }

            return languageCode.Trim().ToLowerInvariant();
        }
    }
}
