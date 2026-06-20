using Hangman.Business.Messages;
using Hangman.Business.Results;
using Hangman.Contracts.Match;

namespace Hangman.Business.Validators
{
    public static class ScoreValidator
    {
        public static ValidationResult ValidateGetPlayerScore(GetPlayerScoreRequest request)
        {
            if (request == null)
            {
                return ValidationResult.Fail(ScoreMessageCode.PlayerNotFound);
            }

            if (request.PlayerId <= 0)
            {
                return ValidationResult.Fail(ScoreMessageCode.PlayerNotFound);
            }

            return ValidationResult.Success();
        }
    }
}
