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
            catch (AddressAccessDeniedException ex)
            {
                Log.Fatal("Access denied while opening service host. Check URL ACL permissions.", ex);
                AbortHost(host);
                throw;
            }
            catch (AddressAlreadyInUseException ex)
            {
                Log.Fatal("Address already in use while opening service host.", ex);
                AbortHost(host);
                throw;
            }
            catch (TimeoutException ex)
            {
                Log.Fatal("Timeout while opening service host.", ex);
                AbortHost(host);
                throw;
            }
            catch (CommunicationException ex)
            {
                Log.Fatal("Communication error while opening service host.", ex);
                AbortHost(host);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Log.Fatal("Invalid service host configuration.", ex);
                AbortHost(host);
                throw;
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
