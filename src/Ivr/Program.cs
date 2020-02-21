using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using System;
using System.IO;
using static System.Console;
using System.Collections.Generic;
using System.Linq;

using System.Text.Json;
using System.Text.Json.Serialization;

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

            var rdps = app.Node.Resolve(ctx, "RDPs", help: "expected as comma-separated list of IPv4 CIDRs")
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            // ingress is for RDP and SIP only
            var ingressRules = rdps.Select(x => new IngressRule(Peer.Ipv4(x.Trim()), Port.Tcp(3389), $"RDP"))
                .Concat(SipProviders.Select(region));
            
            var ec2users = app.Node.Resolve(ctx, "Ec2users")
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                ?.Select(u => u.Trim());

            var ivrStackProps = new IvrStackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = account,
                    Region = region,
                },
                IngressRules = ingressRules,
                KeyPairName = app.Node.Resolve(ctx, "KeyPairName"),
                RdpUserName = app.Node.Resolve(ctx, "RdpUserName"),
                RdpUserPassword = app.Node.Resolve(ctx, "RdpUserPassword"),
                BucketsDomain = app.Node.Resolve(ctx, "BucketsDomain"),
                EC2Users = ec2users,
                s3i_args = app.Node.Resolve(ctx, "s3i_args"),
            };
            new IvrStack(app, "IvrStack", ivrStackProps);
            
            app.Synth();
        }
    }
}
