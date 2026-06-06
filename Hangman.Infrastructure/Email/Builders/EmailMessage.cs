namespace Hangman.Infrastructure.Email.Builders
{
    internal class EmailMessage
    {
        public EmailMessage(string recipient, string subject, string body, bool isBodyHtml)
        {
            Recipient = recipient ?? string.Empty;
            Subject = subject ?? string.Empty;
            Body = body ?? string.Empty;
            IsBodyHtml = isBodyHtml;
        }

        public string Recipient { get; private set; }

        public string Subject { get; private set; }

        public string Body { get; private set; }

        public bool IsBodyHtml { get; private set; }
    }
}
