using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.RegionInfo;

using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

using IvrLib;
using IvrLib.Security;
using IvrLib.SipProviders;
using IvrLib.Utils;

namespace Ivr
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App(new AppProps { });

            // can't rely on (incorrect) CDK implementation, so read these files one by one, values from previous may be overridden by those from next
            var ctx = Context.FromJsonFiles(app.Node, $"{OSAgnostic.Home}/cdk.json", $"cdk.json", app.Node.TryGetContext("ctx") as string);

            // Mandatory parameters are not a part of the schema
            var accountNumber = ctx.Resolve("account", "CDK_DEFAULT_ACCOUNT");//, $"Please provide account number: -c account=<number>");
            if(string.IsNullOrWhiteSpace(accountNumber)) {
                accountNumber = new AmazonSecurityTokenServiceClient().GetCallerIdentityAsync(new GetCallerIdentityRequest()).Result.Account;
                if(string.IsNullOrWhiteSpace(accountNumber)) {
                    throw new ArgumentException($"No account number returned by AWS STS");
                }
            }
            var regionName = ctx.Resolve("region", "CDK_DEFAULT_REGION", $"Please provide AWS region: -c region=<region>");
            var regionInfo = RegionInfo.Get(regionName);
            Console.WriteLine($"Account: {accountNumber}/{regionInfo?.Name}, {regionInfo?.DomainSuffix}");
            if(string.IsNullOrWhiteSpace(regionInfo?.DomainSuffix)) throw new ArgumentException($"Invalid region: '{regionName}'");

            var schemaFilePath = ctx.Resolve("schema", help: "Schema file path required: -c schema=<path_to_schema_yaml>");
            Console.WriteLine($"Schema [{schemaFilePath}]");
            IvrSiteSchema schema;
            using (var sr = new StreamReader(schemaFilePath))
            {
                var ext = Path.GetExtension(schemaFilePath).ToLower();
                if (".yaml" == ext)
                {
                    schema = IvrSiteSchema.FromString(System.Environment.ExpandEnvironmentVariables(sr.ReadToEnd()));
                    Console.WriteLine(new YamlDotNet.Serialization.SerializerBuilder().Build().Serialize(schema));
                    if(string.IsNullOrWhiteSpace(schema.SiteName)) {
                        var fileName = Path.GetFileNameWithoutExtension(schemaFilePath);
                        schema.SiteName = fileName;
                    }
                }
                else
                {
                    throw new ArgumentException($"Handling of *{ext} format is not implemented (yet)");
                }
                schema.Resolve(ctx).Validate().Preprocess();
            }
            var rdpIngressRules = schema.RdpProps.Cidrs.Select(x => new IngressRule(Peer.Ipv4(x.Trim()), Port.Tcp(3389), "RDP").WithDescription($"RDP client"));
            var sipIngressRules = SipProviders.Select(regionInfo.Name, schema.SipProviders, schema.IngressPorts);
            new IvrStack(app, $"IvrStack-{schema.SiteName}".AsCloudFormationId(), 
                new StackProps
                {
                    Env = new Amazon.CDK.Environment
                    {
                        Account = accountNumber,
                        Region = regionInfo.Name,
                    },
                    Tags = schema.Tags,
                },
                schema,
                rdpIngressRules.Concat(sipIngressRules)
            );
            app.Synth();
        }
    }
}
