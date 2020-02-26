using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class SipProviders
    {
        public static IEnumerable<SecurityGroupRule> Select(string region, IEnumerable<string> providers, IEnumerable<PortSpec> ingressPorts)
        {
            return Providers.Aggregate(new List<SecurityGroupRule>(), (list, provider) =>
            {
                if (null == providers || provider.NameMatchesAny(providers))
                {
                    list.AddRange(provider.Select(region, ingressPorts));
                }
                return list;
            });
        }
        public static IEnumerable<string> KnownProviderNames { get { return Providers.Select(p => p.Name); } }
        //
        // Here we need to define only Egress rules, Ingress will be derived from these during selection
        //  but explicit Igress rules can also be added here
        protected static IEnumerable<SipProvider> Providers = new List<SipProvider>
        {
            // https://d1.awsstatic.com/whitepapers/leveraging_chime_voice_connector_for_sip_trunking.pdf (Appendix B)
            //   that's all - as of 2/20/2020
            new SipProvider("Amazon", "Amazon Chime Voice Connector", new Dictionary<string, IEnumerable<SecurityGroupRule>> {
                    { "us-east-1", new List<SecurityGroupRule> {
                        new EgressRule(Peer.Ipv4("3.80.16.0/23"), Port.UdpRange(5000, 65000), "SIP", "RTP"),
                        new EgressRule(Peer.Ipv4("52.55.62.128/25"), Port.UdpRange(1024, 65535), "SIP", "RTP"),
                        new EgressRule(Peer.Ipv4("52.55.63.0/25"), Port.UdpRange(1024, 65535), "SIP", "RTP"),
                        new EgressRule(Peer.Ipv4("34.212.95.128/25"), Port.UdpRange(1024, 65535), "SIP", "RTP"),
                        new EgressRule(Peer.Ipv4("34.223.21.0/25"), Port.UdpRange(1024, 65535), "SIP", "RTP"),
                    }},
                    { "us-west-2", new List<SecurityGroupRule> {
                        new EgressRule(Peer.Ipv4("99.77.253.0/24"), Port.UdpRange(5000, 65000), "SIP", "RTP"),
                    }}
                }
            ),    
            // https://www.twilio.com/docs/sip-trunking#termination
            //   they have more - see the list, map to AWS Regions, and add below
            new SipProvider("Twilio", "Twilio Elastic SIP Trunking", new Dictionary<string, IEnumerable<SecurityGroupRule>> {
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
                    }}
                }
            ),
            // https://www.plivo.com/docs/sip-trunking/zentrunk-quickstart#creating-an-inbound-trunk
            // https://support.plivo.com/support/solutions/articles/17000012097-what-ip-addresses-do-i-need-to-whitelist-on-my-communications-infrastructure-for-zentrunk-sip-trunkin
            //   again, see the link for full list
            new SipProvider("Plivo", "Plivo Zentrunk SIP Trunking", new Dictionary<string, IEnumerable<SecurityGroupRule>> {
                    { "us-east-1", new List<SecurityGroupRule> {
                        new EgressRule(Peer.Ipv4("18.214.109.128/25"), Port.Udp(5060), "SIP"),
                        new EgressRule(Peer.Ipv4("18.214.109.128/25"), Port.UdpRange(10000, 30000), "RTP"),
                        new EgressRule(Peer.Ipv4("18.215.142.0/26"), Port.Udp(5060), "SIP"),
                        new EgressRule(Peer.Ipv4("18.215.142.0/26"), Port.UdpRange(10000, 30000), "RTP"),
                    }},
                    { "us-west-1", new List<SecurityGroupRule> {
                        new EgressRule(Peer.Ipv4("13.52.9.0/25"), Port.Udp(5060), "SIP"),
                        new EgressRule(Peer.Ipv4("13.52.9.0/25"), Port.UdpRange(10000, 20000), "RTP"),
                    }},
                    { "ap-southeast-2", new List<SecurityGroupRule> {
                        new EgressRule(Peer.Ipv4("13.238.202.192/26"), Port.Udp(5060), "SIP"),
                        new EgressRule(Peer.Ipv4("13.238.202.192/26"), Port.UdpRange(10000, 20000), "RTP"),
                    }},
                }
            ),
        };
    }
}