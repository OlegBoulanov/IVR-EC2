using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class AmazonChimeVoiceConnector : SipProvider
    {
        // https://d1.awsstatic.com/whitepapers/leveraging_chime_voice_connector_for_sip_trunking.pdf (Appendix B)
        //   that's all they have, as of 2/20/2020
        public AmazonChimeVoiceConnector() : base("Amazon", "Chime Voice Connector", new Dictionary<string, IEnumerable<SecurityGroupRule>> {
            // Here we need to define only Egress rules, Ingress will be derived from these during selection
            //  but explicit Igress rules can also be added here
            { "us-east-1", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("3.80.16.0/23"), Port.UdpRange(5000, 65000), "SIP", "RTP"),
                new EgressRule(Peer.Ipv4("52.55.62.128/25"), Port.UdpRange(1024, 65535), "SIP", "RTP"),
                new EgressRule(Peer.Ipv4("52.55.63.0/25"), Port.UdpRange(1024, 65535), "SIP", "RTP"),
                new EgressRule(Peer.Ipv4("34.212.95.128/25"), Port.UdpRange(1024, 65535), "SIP", "RTP"),
                new EgressRule(Peer.Ipv4("34.223.21.0/25"), Port.UdpRange(1024, 65535), "SIP", "RTP"),
            }},
            { "us-west-2", new List<SecurityGroupRule> {
                new EgressRule(Peer.Ipv4("99.77.253.0/24"), Port.UdpRange(5000, 65000), "SIP", "RTP"),
            }},
        }) {}
    }
}