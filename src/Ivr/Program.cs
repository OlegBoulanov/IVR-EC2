using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;

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

            var account = app.Node.Resolve(ctx, "account", "CDK_DEFAULT_ACCOUNT");
            var region = app.Node.Resolve(ctx, "region", "CDK_DEFAULT_REGION");
            var comment = app.Node.Resolve(ctx, "comment") ?? "no comments";
            System.Console.WriteLine($"{account}/{region}, {comment}");

            var rdps = app.Node.Resolve(ctx, "RdpCIDRs", help: "expected as comma-separated list of IPv4 CIDRs, like '73.118.72.189/32, 54.203.115.236/32'").Csv();
            
            // we expect to have an RDP connection
            var keyPairName = app.Node.Resolve(ctx, "KeyPairName");
            var rdpUserName = app.Node.Resolve(ctx, "RdpUserName");
            var rdpUserPassword = app.Node.Resolve(ctx, "RdpUserPassword");
            if(string.IsNullOrWhiteSpace(keyPairName) && string.IsNullOrWhiteSpace(rdpUserName)) throw new ArgumentNullException($"RdpUserName, or KeyPairName (to retrieve Administrator account password later) is required");

            // Ingress traffic open for RDP and inbound SIP providers only
            var rdpIngressRules = rdps.Select(x => new IngressRule(Peer.Ipv4(x.Trim()), Port.Tcp(3389), $"RDP client"));
            var voipIngressPorts = new List<Port> { 
                Port.UdpRange(15060, 15062), 
                Port.UdpRange(15064, 15320),
            };
            var voipIngressRules = SipProviders.Select(region, app.Node.Resolve(ctx, "SipProviders")?.Csv(), voipIngressPorts);
            if(0 == voipIngressRules.Count()) throw new ArgumentNullException($"Region {region} seem not having any SIP providers");
            
            var ec2users = app.Node.Resolve(ctx, "Ec2users")?.Csv();

            var ivrStackProps = new IvrStackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = account,
                    Region = region,
                },
                SecurityGroupRules = rdpIngressRules.Concat(voipIngressRules),
                KeyPairName = keyPairName,
                RdpUserName = rdpUserName,
                RdpUserPassword = rdpUserPassword,
                BucketsDomain = app.Node.Resolve(ctx, "BucketsDomain"),
                EC2Users = ec2users,
                s3i_args = app.Node.Resolve(ctx, "s3i_args"),
            };
            new IvrStack(app, "IvrStack", ivrStackProps);

            app.Synth();
        }
    }
}
