using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using log4net;

namespace ServerAlive
{
    static class NetworkStatus
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(NetworkStatus));

        public static void ShowNetworkInterfaces()
        {
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            _log.Info($"Interface information for {computerProperties.HostName}.{computerProperties.DomainName}  ");
            if (nics == null || nics.Length < 1)
            {
                _log.Info("  No network interfaces found.  ");
                return;
            }

            _log.Info($"  Number of interfaces .................... : {nics.Length}");
            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                string versions = "";
                _log.Info("");
                _log.Info(adapter.Description);
                _log.Info(string.Empty.PadLeft(adapter.Description.Length, '='));
                _log.Info($"  Interface type .......................... : {adapter.NetworkInterfaceType}");
                _log.Info($"  Physical Address ........................ : {adapter.GetPhysicalAddress().ToString()}");
                _log.Info($"  Operational status ...................... : {adapter.OperationalStatus}");
                if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    versions = "IPv4";
                }
                if (adapter.Supports(NetworkInterfaceComponent.IPv6))
                {
                    if (versions.Length > 0)
                    {
                        versions += " ";
                    }
                    versions += "IPv6";
                }
                _log.Info($"  IP version .............................. : {versions}");
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }
                _log.Info($"  DNS suffix .............................. : {properties.DnsSuffix}");
                if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    IPv4InterfaceProperties ipv4 = properties.GetIPv4Properties();
                    _log.Info($"  MTU...................................... : {ipv4.Mtu}");
                }
                _log.Info($"  DNS enabled ............................. : {properties.IsDnsEnabled}");
                _log.Info($"  Dynamically configured DNS .............. : {properties.IsDynamicDnsEnabled}");
                _log.Info($"  Receive Only ............................ : {adapter.IsReceiveOnly}");
                _log.Info($"  Multicast ............................... : {adapter.SupportsMulticast}");
                //ShowInterfaceStatistics(adapter);
                _log.Info("");
            }
        }

        private static List<IPAddress> GetDnsAddress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            List<IPAddress> adresses = new List<IPAddress>();
            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;
                    foreach (IPAddress dnsAdress in dnsAddresses)
                    {
                        adresses.Add(dnsAdress);
                    }
                }
            }
            return adresses;
        }

        private static bool Ping(string address)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = "PingMessage!";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            try
            {
                PingReply reply = pingSender.Send(address, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception)
            {
                return false;
            }
        }

        public static void CheckInternetConnection()
        {
            _log.Info("");
            _log.Info("");
            _log.Info("");
            _log.Info("  Internet status connection");
            _log.Info(string.Empty.PadLeft("  Internet status connection".Length, '='));
            List<IPAddress> dnsAdresses = GetDnsAddress();
            if (dnsAdresses.Count == 0)
            {
                _log.Info("  DNS Address ...................... : Not Available");
                _log.Info("  Internet connection .............. : FALSE");
            }
            else
            {
                int countSuccessConnection = 0;
                foreach (var address in dnsAdresses)
                {
                    _log.Info($"  DNS Address ...................... : {address}");
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        countSuccessConnection += Ping(address.ToString()) ? 1 : 0;
                    }
                }
                if (countSuccessConnection > 0)
                {
                    _log.Info("  Internet connection ...................... : TRUE");
                }
                else
                {
                    _log.Info("  Internet connection ...................... : FALSE");
                }
            }
            _log.Info("");
            _log.Info("");
            _log.Info("");
        }
    }
}
