using log4net;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Hangman.ConsoleHost.Hosting
{
    public sealed class ServiceHostManager : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceHostManager));

        private readonly List<ServiceHost> hosts;

        public ServiceHostManager()
        {
            hosts = new List<ServiceHost>();
        }

        public void Add(ServiceHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            hosts.Add(host);
        }

        public void OpenAll()
        {
            foreach (ServiceHost host in hosts)
            {
                OpenHost(host);
            }
        }

        public void Dispose()
        {
            foreach (ServiceHost host in hosts)
            {
                CloseHost(host);
            }
        }

        private static void OpenHost(ServiceHost host)
        {
            try
            {
                host.Open();

                Console.WriteLine("Service started: {0}", host.BaseAddresses[0]);
                Log.InfoFormat("Service started: {0}", host.BaseAddresses[0]);
            }
            catch (AddressAccessDeniedException exception)
            {
                AbortHost(host);

                throw new InvalidOperationException(
                    "Access denied while opening service host. Check URL ACL permissions.",
                    exception);
            }
            catch (AddressAlreadyInUseException exception)
            {
                AbortHost(host);

                throw new InvalidOperationException(
                    "Address already in use while opening service host.",
                    exception);
            }
            catch (TimeoutException exception)
            {
                AbortHost(host);

                throw new InvalidOperationException(
                    "Timeout while opening service host.",
                    exception);
            }
            catch (CommunicationException exception)
            {
                AbortHost(host);

                throw new InvalidOperationException(
                    "Communication error while opening service host.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                AbortHost(host);

                throw new InvalidOperationException(
                    "Invalid service host configuration.",
                    exception);
            }
        }

        private static void CloseHost(ServiceHost host)
        {
            if (host == null)
            {
                return;
            }

            try
            {
                if (host.State == CommunicationState.Opened)
                {
                    host.Close();
                    Log.InfoFormat("Service closed: {0}", host.BaseAddresses[0]);
                    return;
                }

                AbortHost(host);
            }
            catch (TimeoutException ex)
            {
                Log.Warn("Timeout while closing service host. Host will be aborted.", ex);
                AbortHost(host);
            }
            catch (CommunicationException ex)
            {
                Log.Warn("Communication error while closing service host. Host will be aborted.", ex);
                AbortHost(host);
            }
            catch (ObjectDisposedException ex)
            {
                Log.Warn("Service host was already disposed.", ex);
            }
            catch (InvalidOperationException ex)
            {
                Log.Warn("Invalid host state while closing service host. Host will be aborted.", ex);
                AbortHost(host);
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected error while closing service host. Host will be aborted.", ex);
                AbortHost(host);
            }
        }

        private static void AbortHost(ServiceHost host)
        {
            try
            {
                if (host != null)
                {
                    host.Abort();
                }
            }
            catch (Exception exception)
            {
                Log.Error("Unexpected error while aborting service host.", exception);
            }
        }
    }
}
