using Hangman.Business.Messages;
using System;

namespace Hangman.Business.Results
{
    public class ValidationResult
    {
        public bool IsValid { get; private set; }

        public Enum MessageCode { get; private set; }

        private ValidationResult(bool isValid, Enum messageCode)
        {
            IsValid = isValid;
            MessageCode = messageCode;
        }

        public static ValidationResult Success()
        {
            return new ValidationResult(true, null);
        }

        public static ValidationResult Fail(AuthMessageCode messageCode)
        {
            return FailMessage(messageCode);
        }

        public static ValidationResult Fail(ProfileMessageCode messageCode)
        {
            return FailMessage(messageCode);
        }

        public static ValidationResult Fail(ScoreMessageCode messageCode)
        {
            return FailMessage(messageCode);
        }

        private static ValidationResult FailMessage(Enum messageCode)
        {
            if (messageCode == null)
            {
                throw new ArgumentNullException(nameof(messageCode));
            }

            return new ValidationResult(false, messageCode);
        }
    }
}
