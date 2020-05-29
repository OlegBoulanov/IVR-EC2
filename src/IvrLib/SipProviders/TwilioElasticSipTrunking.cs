using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

using IvrLib.Security;

namespace IvrLib.SipProviders
{
    public class TwilioElasticSipTrunking : SipProvider
    {
        // https://www.twilio.com/docs/sip-trunking#termination
        // there is more for other regions
        public TwilioElasticSipTrunking() : base("Twilio", "Elastic SIP Trunking", new Dictionary<string, IEnumerable<SecurityGroupRule>> {
            // Here we need to define only Egress rules, Ingress will be derived from these during selection
            //  but explicit Igress rules can also be added here
            { "us-east-1", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("54.172.60.0/30"), Port.Udp(5060), "SIP"),
                new EgressRule(Peer.Ipv4("54.172.60.0/23"), Port.UdpRange(10000, 20000), "RTP"),
                new EgressRule(Peer.Ipv4("34.203.250.0/23"), Port.UdpRange(10000, 20000), "RTP"),
            }},
            { "us-west-2", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("54.244.51.0/30"), Port.Udp(5060), "SIP"),
                new EgressRule(Peer.Ipv4("54.244.51.0/30"), Port.Tcp(5061), "SIPS"),
                new EgressRule(Peer.Ipv4("54.244.51.0/24"), Port.UdpRange(10000, 20000), "RTP"),
            }},
            { "ap-southeast-2", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("54.252.254.64/30"), Port.Udp(5060), "SIP"),
                new EgressRule(Peer.Ipv4("54.252.254.64/30"), Port.Tcp(5061), "SIPS"),
                new EgressRule(Peer.Ipv4("54.252.254.64/26"), Port.UdpRange(10000, 20000), "RTP"),
                new EgressRule(Peer.Ipv4("3.104.90.0/24"), Port.UdpRange(10000, 20000), "RTP"),
            }},
        }) {}
    }
}