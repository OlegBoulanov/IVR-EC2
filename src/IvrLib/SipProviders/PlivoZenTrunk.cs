using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

using IvrLib.Security;

namespace IvrLib.SipProviders
{
    public class PlivoZenTrunk : SipProvider
    {
        // https://www.plivo.com/docs/sip-trunking/zentrunk-quickstart#creating-an-inbound-trunk
        // https://support.plivo.com/support/solutions/articles/17000012097-what-ip-addresses-do-i-need-to-whitelist-on-my-communications-infrastructure-for-zentrunk-sip-trunkin
        // https://www.plivo.com/docs/sip-trunking#ip-address-whitelisting
        // there is more for other regions
        public PlivoZenTrunk() : base("Plivo", "Zentrunk SIP Trunking", new Dictionary<string, IEnumerable<SecurityGroupRule>> {
            // Here we need to define only Egress rules, Ingress will be derived from these during selection
            //  but explicit Igress rules can also be added here
            { "us-east-1", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("18.214.109.128/25"), Port.Udp(5060), "SIP"),
                new EgressRule(Peer.Ipv4("18.214.109.128/25"), Port.UdpRange(10000, 30000), "RTP"),
                new EgressRule(Peer.Ipv4("18.215.142.0/26"), Port.Udp(5060), "SIP"),
                new EgressRule(Peer.Ipv4("18.215.142.0/26"), Port.UdpRange(10000, 30000), "RTP"),
            }},
            { "us-west-1", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("13.52.9.0/25"), Port.Udp(5060), "SIP"),
                new EgressRule(Peer.Ipv4("13.52.9.0/25"), Port.UdpRange(10000, 30000), "RTP"),
            }},
            { "ap-southeast-2", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("13.238.202.192/26"), Port.Udp(5060), "SIP"),
                new EgressRule(Peer.Ipv4("13.238.202.192/26"), Port.UdpRange(10000, 30000), "RTP"),
            }},
        }) {}
    }
}