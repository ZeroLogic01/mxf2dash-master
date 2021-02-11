using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Slave
{
    static class ListenerSocketFactory
    {

        private const string CouldNotGetIpAddressError = "Couldn`t get an IP address";

        private static IPAddress _selfIP = null;
        public static IPAddress SelfIP { get => _selfIP; }

        private static bool CanBind(IPAddress address,int port)
        {
            try
            {
                IPEndPoint ep = new IPEndPoint(address, port);
                using (Socket s = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    s.Bind(ep);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }
        private static IPAddress GetIPByInteraface(int portToTryBind)
        {
            IPAddress defaultAddress = IPAddress.Any;

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in networkInterfaces)
            {
                if ((ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet || ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                {
                    var ips = ni.GetIPProperties().UnicastAddresses;
                    foreach (var ip in ips)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (CanBind(ip.Address, portToTryBind))
                            {
                                _selfIP = ip.Address;
                                return ip.Address;
                            }
                        }
                    }
                }
            }
            return defaultAddress;
        }
        public static Socket Create(int port)
        {
            IPAddress address = CanBind(SelfIP, port) ? SelfIP : GetIPByInteraface(port);

            if (address.Equals(IPAddress.Any))
            {
                throw new Exception(CouldNotGetIpAddressError);
            }

            IPEndPoint endPoint = new IPEndPoint(address, port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endPoint);

            return socket;
        }
    }
}
