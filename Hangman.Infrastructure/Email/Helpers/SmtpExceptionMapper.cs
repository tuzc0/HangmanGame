using Hangman.Business.Email;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
namespace Hangman.Infrastructure.Email.Helpers
{
    internal static class SmtpExceptionMapper
    {
        private const int SocketTimedOut = 10060;

        public static EmailSendResult Map(SmtpException exception)
        {
            if (exception == null)
            {
                return EmailSendResult.PermanentFailure(EmailErrorCode.SmtpUnknown, null);
            }

            if (IsTimeout(exception))
            {
                return EmailSendResult.TemporaryFailure(EmailErrorCode.SmtpTimeout, exception);
            }

            if (IsAuthenticationError(exception))
            {
                return EmailSendResult.PermanentFailure(EmailErrorCode.SmtpAuthenticationFailed, exception);
            }

            if (exception.StatusCode == SmtpStatusCode.MustIssueStartTlsFirst ||
                exception.StatusCode == SmtpStatusCode.CommandNotImplemented)
            {
                return EmailSendResult.PermanentFailure(EmailErrorCode.SmtpConfigurationError, exception);
            }

            if (exception.StatusCode == SmtpStatusCode.GeneralFailure ||
                exception.StatusCode == SmtpStatusCode.TransactionFailed ||
                exception.StatusCode == SmtpStatusCode.MailboxBusy ||
                exception.StatusCode == SmtpStatusCode.InsufficientStorage)
            {
                return EmailSendResult.TemporaryFailure(EmailErrorCode.SmtpUnavailable, exception);
            }

            return EmailSendResult.PermanentFailure(EmailErrorCode.SmtpUnknown, exception);
        }

        private static bool IsTimeout(SmtpException exception)
        {
            if (exception.InnerException is TimeoutException)
            {
                return true;
            }

            WebException webException = exception.InnerException as WebException;

            if (webException != null && webException.Status == WebExceptionStatus.Timeout)
            {
                return true;
            }

            SocketException socketException = exception.InnerException as SocketException;

            if (socketException != null && socketException.ErrorCode == SocketTimedOut)
            {
                return true;
            }

            return false;
        }

        private static bool IsAuthenticationError(SmtpException exception)
        {
            string message = (exception.Message ?? string.Empty).ToLowerInvariant();

            return message.Contains("auth") ||
                   message.Contains("login") ||
                   message.Contains("535") ||
                   exception.StatusCode == SmtpStatusCode.ClientNotPermitted;
        }
    }
}
