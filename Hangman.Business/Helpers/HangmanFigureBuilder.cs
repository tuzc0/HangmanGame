using Hangman.Contracts.Match;

namespace Hangman.Business.Helpers
{
    internal static class HangmanFigureBuilder
    {
        public static HangmanFigureDto Build(
            int failedAttempts,
            int maxAttempts)
        {
            return new HangmanFigureDto
            {
                FailedAttempts = failedAttempts,
                MaxAttempts = maxAttempts,
                ShowHead = failedAttempts >= 1,
                ShowTorso = failedAttempts >= 2,
                ShowLeftArm = failedAttempts >= 3,
                ShowRightArm = failedAttempts >= 4,
                ShowLeftLeg = failedAttempts >= 5,
                ShowRightLeg = failedAttempts >= 6
            };
        }
    }
}
