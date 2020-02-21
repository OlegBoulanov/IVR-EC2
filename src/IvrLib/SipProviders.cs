using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class SipProviders
    {
        public static IEnumerable<IngressRule> Select(string region)
        {
            return Providers.Aggregate(new List<IngressRule>(), (list, provider) =>
            {
                list.AddRange(provider.Select(region));
                return list;
            });
        }
        public const string SipSignalling = "(SIP/Signalling)";
        public const string RtpMedia = "(RTP/Media)";
        public const string SipRtp = "(SIP/RTP)";
        protected static List<SipProvider> Providers = new List<SipProvider>
        {
            // https://docs.aws.amazon.com/chime/latest/ag/network-config.html#cvc
            //   that's all - as of 2/20/2020
            new SipProvider("Amazon", "Amazon Chime Voice Connector",new Dictionary<string, IEnumerable<IngressRule>> {
                    { "us-east-1", new List<IngressRule> {
                        new IngressRule(Peer.Ipv4("3.80.16.0/23"), Port.UdpRange(5000, 65000), SipRtp),
                        new IngressRule(Peer.Ipv4("52.55.62.128/25"), Port.UdpRange(1024, 65535), SipRtp),
                        new IngressRule(Peer.Ipv4("52.55.63.0/25"), Port.UdpRange(1024, 65535), SipRtp),
                        new IngressRule(Peer.Ipv4("34.212.95.128/25"), Port.UdpRange(1024, 65535), SipRtp),
                        new IngressRule(Peer.Ipv4("34.223.21.0/25"), Port.UdpRange(1024, 65535), SipRtp),
                    }},
                    { "us-west-2", new List<IngressRule> {
                        new IngressRule(Peer.Ipv4("99.77.253.0/24"), Port.UdpRange(5000, 65000), SipRtp),
                    }}
                }
            ),    
            // https://www.twilio.com/docs/sip-trunking/configure-with-interconnect#IPwhitelist-tnx
            //   they have more - see the list, map to AWS Regions, and add below
            new SipProvider("Twilio", "Twilio Elastic SIP Trunking", new Dictionary<string, IEnumerable<IngressRule>> {
                    { "us-east-1", new List<IngressRule> {
                        new IngressRule(Peer.Ipv4("208.78.112.64/30"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("208.78.112.64/26"), Port.UdpRange(10000, 20000), RtpMedia),
                    }},
                    { "us-west-2", new List<IngressRule> {
                        new IngressRule(Peer.Ipv4("67.213.136.64/30"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("67.213.136.64/26"), Port.UdpRange(10000, 20000), RtpMedia),
                    }},
                    { "ap-southeast-2", new List<IngressRule> {
                        new IngressRule(Peer.Ipv4("103.146.214.68/30"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("103.146.214.64/26"), Port.UdpRange(10000, 20000), RtpMedia),
                    }}
                }
            ),
            // https://www.plivo.com/docs/voice/concepts/ip-address-whitelisting/
            //   again, see the link for full list
            new SipProvider("Plivo", "Plivo SIP Trunking",new Dictionary<string, IEnumerable<IngressRule>> {
                    { "us-east-1", new List<IngressRule> {
                        new IngressRule(Peer.Ipv4("54.215.5.82/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("107.20.176.37/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("107.20.251.237/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("184.169.138.133/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("3.93.158.128/25"), Port.UdpRange(16384, 32768), RtpMedia),
                        new IngressRule(Peer.Ipv4("52.205.63.192/26"), Port.UdpRange(16384, 32768), RtpMedia),
                    }},
                    { "us-west-1", new List<IngressRule> {
                        new IngressRule(Peer.Ipv4("54.215.5.82/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("107.20.176.37/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("107.20.251.237/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("184.169.138.133/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("52.9.254.64/26"), Port.UdpRange(16384, 32768), RtpMedia),
                    }},
                    { "ap-southeast-2", new List<IngressRule> {
                        new IngressRule(Peer.Ipv4("54.215.5.82/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("107.20.176.37/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("107.20.251.237/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("184.169.138.133/32"), Port.Udp(5060), SipSignalling),
                        new IngressRule(Peer.Ipv4("52.65.191.160/27"), Port.UdpRange(16384, 32768), RtpMedia),
                        new IngressRule(Peer.Ipv4("52.65.127.160/27"), Port.UdpRange(16384, 32768), RtpMedia),
                    }},
                }
            ),
        };
    }
}