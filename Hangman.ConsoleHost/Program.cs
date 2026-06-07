using Hangman.ConsoleHost.Configuration;
using Hangman.ConsoleHost.Hosting;
using log4net.Config;
using System;

namespace Hangman.ConsoleHost
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            HostingSettings settings = new HostingSettingsProvider().GetSettings();
            ServiceHostFactory serviceHostFactory = new ServiceHostFactory(settings);
            ServiceHostRegistry serviceHostRegistry = new ServiceHostRegistry(settings);

            using (ServiceHostManager serviceHostManager = new ServiceHostManager())
            {
                foreach (ServiceHostDefinition definition in serviceHostRegistry.GetServiceDefinitions())
                {
                    serviceHostManager.Add(serviceHostFactory.Create(definition));
                }

                serviceHostManager.OpenAll();

                Console.WriteLine();
                Console.WriteLine("Hangman services are running.");
                Console.WriteLine("Press ENTER to stop services.");
                Console.ReadLine();
            }
        }
    }
}