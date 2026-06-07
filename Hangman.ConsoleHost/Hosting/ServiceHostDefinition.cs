using System;

namespace Hangman.ConsoleHost.Hosting
{
    public class ServiceHostDefinition
    {
        public ServiceHostDefinition(Type serviceType, Type contractType, string address)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ContractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        public Type ServiceType { get; private set; }

        public Type ContractType { get; private set; }

        public string Address { get; private set; }
    }
}
