using Hangman.Business.Messages;

namespace Hangman.Business.Results
{
    public class ValidationResult
    {
        public bool IsValid { get; private set; }

        public AuthMessageCode MessageCode { get; private set; }

        private ValidationResult(bool isValid, AuthMessageCode messageCode)
        {
            IsValid = isValid;
            MessageCode = messageCode;
        }

        public static ValidationResult Success()
        {
            return new ValidationResult(true, default);
        }

        public static ValidationResult Fail(AuthMessageCode messageCode)
        {
            return new ValidationResult(false, messageCode);
        }
    }
}
