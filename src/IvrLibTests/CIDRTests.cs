
using System;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

using IvrLib.Utils;

namespace IvrLibTests
{
    public class CIDRTests
    {
        [Test]
        public void Validation()
        {
            var cidr = CIDR.Parse("10.0.1.0/27");
            Assert.IsTrue(cidr.IsValid);
            Assert.IsTrue(cidr.Includes(IPAddress.Parse("10.0.1.10")));
            Assert.IsTrue(cidr.Includes(IPAddress.Parse("10.0.1.30")));
            Assert.IsFalse(cidr.Includes(IPAddress.Parse("10.0.0.10")));
            Assert.IsFalse(cidr.Includes(IPAddress.Parse("10.0.1.33")));
            Assert.IsTrue(CIDR.IsPrivate(cidr.Address));
            Assert.IsFalse(CIDR.IsPublic(cidr.Address));
            Assert.IsTrue(CIDR.Parse("10.0.0.0/16").IsValid);
            Assert.IsFalse(CIDR.Parse("10.0.1.0/16").IsValid);
        }
        [Test]
        public void IPAddresseExtensions()
        {
            var addr = IPAddress.Parse("192.0.0.1");
            Assert.AreEqual(new byte[] { 192, 0, 0, 1 }, addr.GetAddressBytes());
            var addr1 = addr.Increment(5);
            Assert.AreEqual(new byte[] { 192, 0, 0, 6 }, addr1.GetAddressBytes());
            Assert.IsTrue(IPAddress.Parse("10.0.0.0").IsPrivate());
            Assert.IsTrue(IPAddress.Parse("172.16.0.0").IsPrivate());
            Assert.IsTrue(IPAddress.Parse("172.31.255.255").IsPrivate());
            Assert.IsTrue(IPAddress.Parse("172.32.0.0").IsPublic());
            Assert.IsTrue(IPAddress.Parse("172.200.0.0").IsPublic());
            Assert.IsTrue(IPAddress.Parse("192.168.0.0").IsPrivate());
        }
        [Test]
        public void Addresses2()
        {
            var addr = IPAddress.Parse("192.0.0.1");
            Assert.AreEqual(new byte[] { 192, 0, 0, 1 }, addr.GetAddressBytes());
            Assert.AreEqual(0x01_00_00_c0, BitConverter.ToInt32(addr.GetAddressBytes(), 0));
            var n1 = BitConverter.ToInt32(addr.GetAddressBytes(), 0);
            var n2 = IPAddress.HostToNetworkOrder(n1);
            Assert.AreEqual(0xc0_00_00_01, (uint)n2);
            Assert.AreEqual(0xf0_00_00_00, (uint)0xf<<28);
            Assert.AreEqual(0xf0_00_00_00, (uint)0xf<<28);
            Assert.AreEqual(0xf0, IPAddress.HostToNetworkOrder(-1<<28));
        }
    }
}
