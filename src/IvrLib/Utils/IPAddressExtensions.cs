using System;
using System.Net;
using System.Net.Sockets;

namespace IvrLib.Utils
{
    public static class IPAddressExtensions
    {
        public static IPAddress Increment(this IPAddress addr, int i = 1)
        {
            if (AddressFamily.InterNetwork == addr.AddressFamily)
            {
                return new IPAddress(
                    BitConverter.GetBytes(
                        IPAddress.NetworkToHostOrder(
                            IPAddress.HostToNetworkOrder(
                                BitConverter.ToInt32(addr.GetAddressBytes(), 0)) + i)));
            }
            throw new NotImplementedException($"Not implemented for {addr.AddressFamily} yet");
        }
        public static bool IsPrivate(this IPAddress addr) => CIDR.IsPrivate(addr);
        public static bool IsPublic(this IPAddress addr) => CIDR.IsPublic(addr);
    }
}