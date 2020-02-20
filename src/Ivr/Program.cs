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

            // Configuration values resolution
            // Environment (defaults)
            //  - account
            //  - region
            //  - keypair
            // ~/cdk.json [private zone]
            //  - RDP address(es)
            //  - RDP user name
            //  - RDP user password
            //  - keypair
            // .../cdk[-flavor].json [project/public zone]
            //      - overrides
            // finally, [private] command line -c name=value overrides for specific synth/deployments
            //  these sets of values can also come in files: -c ctx=file

            // can't rely on (incorrect) CDK implementation, so read these files one by one, values from previous may be overridden by those from next
            var ctx = Context.FromJsonFiles($"{OSAgnostic.Home}/cdk.json", $"cdk.json", app.Node.TryGetContext("ctx") as string);

            var account = app.Node.Resolve(ctx, "account", "CDK_DEFAULT_ACCOUNT");
            var region = app.Node.Resolve(ctx, "region", "CDK_DEFAULT_REGION");
            var comment = app.Node.Resolve(ctx, "comment") ?? "no comments";
            System.Console.WriteLine($"{account}/{region}, {comment}");

            var rdps = app.Node.Resolve(ctx, "RDPs", help: "expected as comma-separated list of IPv4 CIDRs")
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var rdpInbounds = rdps.Select(x => new IngressRule { Props = new IngressRuleProps { Peer = Peer.Ipv4(x.Trim()), Connection = Port.Tcp(3389), Description = $"RDP", RemoteRule = false }});
            // can add more inbound CIDR:port pairs here...
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
                IngressRules = rdpInbounds,
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
