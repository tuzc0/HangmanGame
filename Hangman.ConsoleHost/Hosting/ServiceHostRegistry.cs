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
        }

        private static string CombineAddress(string baseAddress, string servicePath)
        {
            return baseAddress.TrimEnd('/') + "/" + servicePath.TrimStart('/');
        }
    }
}
