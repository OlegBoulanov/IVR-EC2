using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.RegionInfo;

using IvrLib;

namespace Ivr
{
    sealed class Program
    {
        public static void Main(string[] args)
        {

            var app = new App();

            // can't rely on (incorrect) CDK implementation, so read these files one by one, values from previous may be overridden by those from next
            var ctx = Context.FromJsonFiles($"{OSAgnostic.Home}/cdk.json", $"cdk.json", app.Node.TryGetContext("ctx") as string);

            var accountNumber = app.Node.Resolve(ctx, "account", "CDK_DEFAULT_ACCOUNT");
            var regionName = app.Node.Resolve(ctx, "region", "CDK_DEFAULT_REGION");
            var regionInfo = RegionInfo.Get(regionName);
            if(null == regionInfo || null == regionInfo.DomainSuffix) throw new ApplicationException($"Invalid region: '{regionName}'");

            System.Console.WriteLine($"{accountNumber}/{regionInfo.Name}, {regionInfo.DomainSuffix}");

            var rdpCIDRs = app.Node.Resolve(ctx, "RdpCIDRs", help: "expected as comma-separated list of IPv4 CIDRs, like '73.118.72.189/32, 54.203.115.236/32'").Csv();
            
            // we expect to have an RDP connection
            var keyPairName = app.Node.Resolve(ctx, "KeyPairName");
            var rdpUserName = app.Node.Resolve(ctx, "RdpUserName");
            var rdpUserPassword = app.Node.Resolve(ctx, "RdpUserPassword");
            if(string.IsNullOrWhiteSpace(keyPairName) && string.IsNullOrWhiteSpace(rdpUserName)) throw new ApplicationException($"RdpUserName, or KeyPairName (to retrieve Administrator account password later) is required");

            // Ingress traffic open for RDP and inbound SIP providers only
            var rdpIngressRules = rdpCIDRs.Select(x => new IngressRule(Peer.Ipv4(x.Trim()), Port.Tcp(3389), $"RDP client"));

            var udpPorts = app.Node.Resolve(ctx, "UdpPorts", help: "expected as comma-separated list of ingress ports, like '5060:Signalling, 5062-5300:Media'").Csv();
            var udpIngressPorts = udpPorts.Aggregate(new List<PortSpec>(), 
                (ports, s) => {
                    var portRangeSpec = PortRangeSpec.Parse(s);
                    if(portRangeSpec.Begin == portRangeSpec.End) ports.Add(new PortSpec { Port = Port.Udp(portRangeSpec.Begin), Description = portRangeSpec.Description, });
                    else ports.Add(new PortSpec { Port = Port.UdpRange(portRangeSpec.Begin, portRangeSpec.End), Description = portRangeSpec.Description, });
                    return ports;
                });

            var udpIngressRules = SipProviders.Select(regionInfo.Name, app.Node.Resolve(ctx, "SipProviders")?.Csv(), udpIngressPorts);
            if(0 == udpIngressRules.Count()) throw new ApplicationException($"Region {regionInfo.Name} seem not having any SIP providers");
            
            var ec2users = app.Node.Resolve(ctx, "Ec2users")?.Csv();

            var ivrStackProps = new IvrStackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = accountNumber,
                    Region = regionInfo.Name,
                },
                RegionInfo = regionInfo,
                SecurityGroupRules = rdpIngressRules.Concat(udpIngressRules),
                KeyPairName = keyPairName,
                RdpUserName = rdpUserName,
                RdpUserPassword = rdpUserPassword,
                BucketsDomain = app.Node.Resolve(ctx, "BucketsDomain"),
                HostsDomainName = app.Node.Resolve(ctx, "HostsDomainName", null, "Existing domain name is expected"),
                EC2Users = ec2users,
                s3i_args = app.Node.Resolve(ctx, "s3i_args"),
            };
            new IvrStack(app, "IvrStack", ivrStackProps);

            app.Synth();
        }
    }
}
