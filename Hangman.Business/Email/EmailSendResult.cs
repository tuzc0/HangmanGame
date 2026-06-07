using System;

namespace Hangman.Business.Email
{
    public class EmailSendResult
    {
        public EmailSendStatus Status { get; private set; }

        public EmailErrorCode ErrorCode { get; private set; }

        public Exception TechnicalException { get; private set; }

        public bool IsSuccess
        {
            get { return Status == EmailSendStatus.Success; }
        }

        private EmailSendResult(
            EmailSendStatus status,
            EmailErrorCode errorCode,
            Exception technicalException)
        {
            Status = status;
            ErrorCode = errorCode;
            TechnicalException = technicalException;
        }

        public static EmailSendResult Success()
        {
            return new EmailSendResult(EmailSendStatus.Success, EmailErrorCode.None, null);
        }

        public static EmailSendResult TemporaryFailure(EmailErrorCode errorCode, Exception technicalException)
        {
            return new EmailSendResult(EmailSendStatus.TemporaryFailure, errorCode, technicalException);
        }

        public static EmailSendResult PermanentFailure(EmailErrorCode errorCode, Exception technicalException)
        {
            return new EmailSendResult(EmailSendStatus.PermanentFailure, errorCode, technicalException);
        }
    }
}
