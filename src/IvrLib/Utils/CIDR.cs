using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

namespace IvrLib.Utils
{
    public class CIDR
    {
        public IPAddress Address { get; set; } = null;
        public int NetMaskBitCount { get; set; } = 0;
        public bool IsValid 
        {
            get
            {
                if (AddressFamily.InterNetwork == Address?.AddressFamily)
                {
                    var bits = BitConverter.ToInt32(Address.GetAddressBytes(), 0); // 192.0.0.1 => 0x01_00_00_c0
                    var mask = 0xffffffff >> (32 - NetMaskBitCount);
                    return 0 == (bits & ~mask);
                }
                throw new NotImplementedException($"Not implemented for {Address?.AddressFamily} yet");
            } 
        }
        public bool Includes(IPAddress a)
        {
            if (AddressFamily.InterNetwork == a?.AddressFamily)
            {
                var ipAddressBytes = BitConverter.ToInt32(a.GetAddressBytes(), 0);
                var cidrAddressBytes = BitConverter.ToInt32(Address.GetAddressBytes(), 0);
                var cidrMaskBytes = IPAddress.HostToNetworkOrder(-1 << (32 - NetMaskBitCount));
                return (ipAddressBytes & cidrMaskBytes) == (cidrAddressBytes & cidrMaskBytes);
            }
            throw new NotImplementedException($"Not implemented for {a?.AddressFamily} yet");
        }
        //
        public override int GetHashCode()
        {
            return (null == Address ? 0 : Address.GetHashCode()) + NetMaskBitCount;
        }
        public override bool Equals(object obj)
        {
            var cidr = obj as CIDR;
            return null != cidr && cidr.Address.Equals(Address) && cidr.NetMaskBitCount.Equals(NetMaskBitCount);
        }
        public override string ToString()
        {
            return $"{Address}/{NetMaskBitCount}";
        }
        public static bool TryParse(string s, out CIDR cidr)
        {
            var p = s.IndexOf('/');
            if (7 /*a.b.c.d*/ <= p
                && IPAddress.TryParse(s.Substring(0, p), out var a) 
                && int.TryParse(s.Substring(p + 1), out var m))
            {
                cidr = new CIDR { Address = a, NetMaskBitCount = m };
                return true;
            }
            cidr = null;
            return false;
        }
        public static CIDR Parse(string s)
        {
            if (TryParse(s, out var cidr)) return cidr;
            throw new FormatException("Invalid CIDR format, <IPv4>/<bits> expected");
        }
        public static bool IsPrivate(IPAddress addr)
        {
            return Private10.Includes(addr) || Private172.Includes(addr) || Private192.Includes(addr);
        }
        public static bool IsPublic(IPAddress addr) 
        {
            return !IsPrivate(addr);
        }
        public static readonly CIDR Private10 = Parse("10.0.0.0/8"), Private172 = Parse("172.16.0.0/12"), Private192 = Parse("192.168.0.0/16");
    }
}