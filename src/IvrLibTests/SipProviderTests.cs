using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

using Amazon.CDK.AWS.EC2;

using IvrLib;

namespace IvrLibTests
{
    public class SipProviderTests
    {
        static SipProvider provider = new SipProvider("Imagine", "Imaginary provider", new Dictionary<string, IEnumerable<SecurityGroupRule>> {
            { "region-1", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("51.132.40.0/30"), Port.Udp(5060), "SIP"),
                new EgressRule(Peer.Ipv4("51.132.40.4/30"), Port.Udp(5061), "SIPS"),
                new EgressRule(Peer.Ipv4("51.132.40.0/23"), Port.UdpRange(10000, 20000), "RTP"),
                new EgressRule(Peer.Ipv4("32.243.25.0/23"), Port.UdpRange(10000, 20000), "RTP"),
            }},
            { "region-2", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("64.214.71.0/30"), Port.Udp(5060), "SIP"),
                new EgressRule(Peer.Ipv4("64.214.71.0/24"), Port.UdpRange(10000, 20000), "RTP"),
            }},
            { "region-3", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("94.212.234.64/30"), Port.Udp(5060), "SIP"),
                new EgressRule(Peer.Ipv4("94.212.234.64/30"), Port.Udp(5061), "SIPS"),
                new EgressRule(Peer.Ipv4("94.212.234.64/26"), Port.UdpRange(10000, 20000), "RTP"),
                new EgressRule(Peer.Ipv4("35.144.190.0/24"), Port.UdpRange(10000, 20000), "RTP"),
            }}
        });

        [Test]
        public void TestRegion3()
        {
            var ingressPorts = new List<PortSpec> { 
                new PortSpec { StartPort = 8060, EndPort = 8060, Protocol = "SIP" }, 
                new PortSpec { StartPort = 9000, EndPort = 9400, Protocol = "RTP" } 
            };
            Assert.AreEqual(0, provider.Select("noregion", ingressPorts).Count());
            var rules = provider.Select("region-3", ingressPorts).ToArray();
            Assert.AreEqual(7, rules.Count());

            Assert.IsTrue(rules[0] is EgressRule);
            Assert.IsTrue(rules[1] is IngressRule);
            Assert.AreEqual(Peer.Ipv4("94.212.234.64/30").UniqueId, rules[1].Peer.UniqueId);
            //Assert.AreEqual("94.212.234.64/30", rules[1].Peer.ToString());  // Amazon.CDK.AWS.EC2.IPeerProxy
            Assert.AreEqual(1, rules[1].Protocols.Count());
            Assert.AreEqual("SIP", rules[1].Protocols[0]);
            Assert.AreEqual("UDP 8060", rules[1].Port.ToString());

            Assert.IsTrue(rules[2] is EgressRule);
            Assert.IsTrue(rules[3] is EgressRule);
            Assert.IsTrue(rules[4] is IngressRule);
            Assert.AreEqual(Peer.Ipv4("94.212.234.64/26").UniqueId, rules[4].Peer.UniqueId);
            Assert.AreEqual(1, rules[4].Protocols.Count());
            Assert.AreEqual("RTP", rules[4].Protocols[0]);
            Assert.AreEqual("UDP 9000-9400", rules[4].Port.ToString());

            Assert.IsTrue(rules[5] is EgressRule);
            Assert.IsTrue(rules[6] is IngressRule);
        }
    }
}
