namespace Hangman.Infrastructure.Email.Builders
{
    internal interface IEmailMessageBuilder<in TContext>
    {
        EmailMessage Build(TContext context);
    }
}
