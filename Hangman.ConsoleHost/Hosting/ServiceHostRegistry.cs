using Hangman.ConsoleHost.Configuration;
using Hangman.Contracts.Contracts;
using HangmanGame.Services.Services;
using System;
using System.Collections.Generic;

namespace Hangman.ConsoleHost.Hosting
{
    public class ServiceHostRegistry
    {
        private readonly HostingSettings settings;

        public ServiceHostRegistry(HostingSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IEnumerable<ServiceHostDefinition> GetServiceDefinitions()
        {
            yield return new ServiceHostDefinition(
                typeof(AuthService),
                typeof(IAuthService),
                CombineAddress(settings.BaseAddress, settings.AuthServicePath));

            yield return new ServiceHostDefinition(
                typeof(ProfileService),
                typeof(IProfileService),
                CombineAddress(settings.BaseAddress, settings.ProfileServicePath));

            yield return new ServiceHostDefinition(
                typeof(WordService),
                typeof(IWordService),
                CombineAddress(settings.BaseAddress, settings.WordServicePath));

            yield return new ServiceHostDefinition(
                typeof(MatchService),
                typeof(IMatchService),
                CombineAddress(settings.BaseAddress, settings.MatchServicePath));

            yield return new ServiceHostDefinition(
                typeof(MatchNotificationService),
                typeof(IMatchNotificationService),
                CombineAddress(
                    settings.DuplexBaseAddress,
                    settings.MatchNotificationServicePath),
                ServiceBindingKind.NetTcpDuplex);

            yield return new ServiceHostDefinition(
                typeof(MatchGameplayService),
                typeof(IMatchGameplayService),
                CombineAddress(settings.BaseAddress, settings.MatchGameplayServicePath));

            yield return new ServiceHostDefinition(
                typeof(MatchGuessService),
                typeof(IMatchGuessService),
                CombineAddress(settings.BaseAddress, settings.MatchGuessServicePath));

            yield return new ServiceHostDefinition(
                typeof(MatchChatService),
                typeof(IMatchChatService),
                CombineAddress(settings.BaseAddress, settings.MatchChatServicePath));

            yield return new ServiceHostDefinition(
                typeof(ScoreService),
                typeof(IScoreService),
                CombineAddress(settings.BaseAddress, settings.ScoreServicePath));
        }

        private static string CombineAddress(string baseAddress, string servicePath)
        {
            return baseAddress.TrimEnd('/') + "/" + servicePath.TrimStart('/');
        }
    }
}
