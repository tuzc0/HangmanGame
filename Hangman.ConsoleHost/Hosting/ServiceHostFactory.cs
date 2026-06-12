using Hangman.ConsoleHost.Configuration;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Hangman.ConsoleHost.Hosting
{
    public class ServiceHostFactory
    {
        private readonly HostingSettings settings;

        public ServiceHostFactory(HostingSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public ServiceHost Create(ServiceHostDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            Uri baseAddress = new Uri(definition.Address);

            ServiceHost host = new ServiceHost(definition.ServiceType, baseAddress);

            if (definition.BindingKind == ServiceBindingKind.NetTcpDuplex)
            {
                host.AddServiceEndpoint(
                    definition.ContractType,
                    CreateNetTcpBinding(),
                    string.Empty);
            }
            else
            {
                host.AddServiceEndpoint(
                    definition.ContractType,
                    CreateBasicHttpBinding(),
                    string.Empty);
            }

            ConfigureMetadata(host, baseAddress);
            ConfigureDebugBehavior(host);

            return host;
        }

        private BasicHttpBinding CreateBasicHttpBinding()
        {
            return new BasicHttpBinding
            {
                MaxReceivedMessageSize = settings.MaxReceivedMessageSize,
                OpenTimeout = TimeSpan.FromSeconds(settings.OpenTimeoutSeconds),
                CloseTimeout = TimeSpan.FromSeconds(settings.CloseTimeoutSeconds),
                SendTimeout = TimeSpan.FromSeconds(settings.SendTimeoutSeconds),
                ReceiveTimeout = TimeSpan.FromMinutes(settings.ReceiveTimeoutMinutes)
            };
        }

        private NetTcpBinding CreateNetTcpBinding()
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            {
                MaxReceivedMessageSize = settings.MaxReceivedMessageSize,
                OpenTimeout = TimeSpan.FromSeconds(settings.OpenTimeoutSeconds),
                CloseTimeout = TimeSpan.FromSeconds(settings.CloseTimeoutSeconds),
                SendTimeout = TimeSpan.FromSeconds(settings.SendTimeoutSeconds),
                ReceiveTimeout = TimeSpan.FromMinutes(settings.ReceiveTimeoutMinutes)
            };

            binding.ReliableSession.Enabled = true;
            binding.ReliableSession.InactivityTimeout =
                TimeSpan.FromMinutes(settings.ReceiveTimeoutMinutes);

            return binding;
        }

        private void ConfigureMetadata(ServiceHost host, Uri baseAddress)
        {
            if (!settings.MetadataEnabled)
            {
                return;
            }

            ServiceMetadataBehavior metadataBehavior =
                host.Description.Behaviors.Find<ServiceMetadataBehavior>();

            if (metadataBehavior == null)
            {
                metadataBehavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(metadataBehavior);
            }

            if (baseAddress.Scheme == Uri.UriSchemeHttp ||
                baseAddress.Scheme == Uri.UriSchemeHttps)
            {
                metadataBehavior.HttpGetEnabled = true;
                metadataBehavior.HttpGetUrl = baseAddress;

                host.AddServiceEndpoint(
                    typeof(IMetadataExchange),
                    MetadataExchangeBindings.CreateMexHttpBinding(),
                    "mex");

                return;
            }

            if (baseAddress.Scheme == Uri.UriSchemeNetTcp)
            {
                host.AddServiceEndpoint(
                    typeof(IMetadataExchange),
                    MetadataExchangeBindings.CreateMexTcpBinding(),
                    "mex");
            }
        }

        private void ConfigureDebugBehavior(ServiceHost host)
        {
            ServiceDebugBehavior debugBehavior =
                host.Description.Behaviors.Find<ServiceDebugBehavior>();

            if (debugBehavior == null)
            {
                debugBehavior = new ServiceDebugBehavior();
                host.Description.Behaviors.Add(debugBehavior);
            }

            debugBehavior.IncludeExceptionDetailInFaults =
                settings.IncludeExceptionDetailInFaults;
        }
    }
}
