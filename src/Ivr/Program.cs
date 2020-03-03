using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.RegionInfo;

using YamlDotNet.Serialization;

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
            var rdpIngressRules = rdpCIDRs.Select(x => new IngressRule(Peer.Ipv4(x.Trim()), Port.Tcp(3389), "RDP").WithDescription($"RDP client"));

            var ingressPorts = app.Node.Resolve(ctx, "IngressPorts", help: "expected as comma-separated list of ingress port ranges, like 'SIP 5060', or 'SIPS 5061, RTP 5062-5300'")
                .Csv()
                .Select(s => PortSpec.Parse(s));

            var udpIngressRules = SipProviders.Select(regionInfo.Name, app.Node.Resolve(ctx, "SipProviders")?.Csv(), ingressPorts);
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

            //var yaml = new SerializerBuilder().Build().Serialize(ivrStackProps);
            //Console.WriteLine(yaml);
            app.Synth();
        }
    }
}
