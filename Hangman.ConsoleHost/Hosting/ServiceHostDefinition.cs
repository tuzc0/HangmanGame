using System;

namespace Hangman.ConsoleHost.Hosting
{
    public class ServiceHostDefinition
    {
        public ServiceHostDefinition(
            Type serviceType,
            Type contractType,
            string address)
            : this(serviceType, contractType, address, ServiceBindingKind.BasicHttp)
        {
        }

        public ServiceHostDefinition(
            Type serviceType,
            Type contractType,
            string address,
            ServiceBindingKind bindingKind)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ContractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
            Address = address;
            BindingKind = bindingKind;
        }

        public Type ServiceType { get; private set; }

        public Type ContractType { get; private set; }

        public string Address { get; private set; }

        public ServiceBindingKind BindingKind { get; private set; }
    }
}
