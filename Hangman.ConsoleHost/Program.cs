using Hangman.ConsoleHost.Configuration;
using Hangman.ConsoleHost.Hosting;
using log4net;
using log4net.Config;
using System;

namespace Hangman.ConsoleHost
{
    internal static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            try
            {
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
            catch (Exception exception)
            {
                Log.Fatal("Hangman services could not be started.", exception);

                Console.WriteLine("Hangman services could not be started. Check logs for details.");
                Environment.ExitCode = 1;
            }
        }
    }
}