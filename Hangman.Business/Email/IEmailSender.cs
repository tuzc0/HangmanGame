using System.Threading.Tasks;

namespace Hangman.Business.Email
{
    public interface IEmailSender
    {
        Task<EmailSendResult> SendVerificationCodeAsync(VerificationEmailRequest request);

        Task<EmailSendResult> SendPasswordResetCodeAsync(PasswordResetEmailRequest request);
    }
}
