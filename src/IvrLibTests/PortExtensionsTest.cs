using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

using Amazon.CDK.AWS.EC2;

using IvrLib.Utils;

namespace IvrLibTests
{
    public class PortExtensionsTest
    {
        [Test]
        public void CloneTest()
        {
            var udp = Port.UdpRange(2000, 3000);
            Assert.AreEqual("UDP 2000-3000", udp.ToString());
            Assert.AreEqual("UDP 2000-3000", udp.Clone().ToString());
            Assert.AreEqual("UDP 2500-3000", udp.Clone(2500).ToString());
            Assert.AreEqual("UDP 2500-2700", udp.Clone(2500, 2700).ToString());
            var tcp = Port.TcpRange(2000, 3000);
            Assert.AreEqual("2000-3000", tcp.ToString());
            Assert.AreEqual("2000-3000", tcp.Clone().ToString());
            Assert.AreEqual("2500-3000", tcp.Clone(2500).ToString());
            Assert.AreEqual("2500-2700", tcp.Clone(2500, 2700).ToString());
        }
        [Test]
        public void ToStringTest()
        {
            Assert.AreEqual("ALL TRAFFIC", Port.AllTraffic().ToString());
            Assert.AreEqual("ALL PORTS", Port.AllTcp().ToString());
            Assert.AreEqual("UDP ALL PORTS", Port.AllUdp().ToString());
            Assert.AreEqual("2000-3000", Port.TcpRange(2000, 3000).ToString());
            Assert.AreEqual("2000", Port.Tcp(2000).ToString());
            Assert.AreEqual("2000-3000", Port.TcpRange(2000, 3000).ToString());
            Assert.AreEqual("UDP 2000", Port.Udp(2000).ToString());
            Assert.AreEqual("UDP 2000-3000", Port.UdpRange(2000, 3000).ToString());
        }
    }
}
