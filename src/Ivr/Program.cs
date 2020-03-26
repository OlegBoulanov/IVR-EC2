using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.RegionInfo;


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
            var ctx = Context.FromJsonFiles($"{OSAgnostic.Home}/cdk.json", $"cdk.json", app.Node.TryGetContext("ctx") as string);

            // Mandatory parameters are not a part of the schema
            var accountNumber = app.Node.Resolve(ctx, "account", "CDK_DEFAULT_ACCOUNT");
            if(string.IsNullOrWhiteSpace(accountNumber)) throw new ArgumentException($"No account number provided");
            var regionName = app.Node.Resolve(ctx, "region", "CDK_DEFAULT_REGION");
            var regionInfo = RegionInfo.Get(regionName);
            Console.WriteLine($"Account: {accountNumber}/{regionInfo?.Name}, {regionInfo?.DomainSuffix}");
            if(string.IsNullOrWhiteSpace(regionInfo?.DomainSuffix)) throw new ArgumentException($"Invalid region: '{regionName}'");

            var schemaFileName = app.Node.Resolve(ctx, "schema", help: "Schema file name required");
            Console.WriteLine($"Schema [{schemaFileName}]");
            IvrSiteSchema schema;
            using (var sr = new StreamReader(schemaFileName))
            {
                var ext = Path.GetExtension(schemaFileName).ToLower();
                if (".yaml" == ext)
                {
                    schema = IvrSiteSchema.FromString(System.Environment.ExpandEnvironmentVariables(sr.ReadToEnd()));
                    Console.WriteLine(new YamlDotNet.Serialization.SerializerBuilder().Build().Serialize(schema));
                    schema.Validate();
                    schema.Preprocess();
                }
                else
                {
                    throw new ArgumentException($"Handling of *{ext} format is not implemented (yet)");
                }
                //throw new ApplicationException("The rest is not implemented yet");
            }
            var rdpIngressRules = schema.RdpProps.Cidrs.Select(x => new IngressRule(Peer.Ipv4(x.Trim()), Port.Tcp(3389), "RDP").WithDescription($"RDP client"));
            var sipIngressRules = SipProviders.Select(regionInfo.Name, schema.SipProviders, schema.IngressPorts);
            new IvrStack(app, "IvrStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = accountNumber,
                    Region = regionInfo.Name,
                },
            }, 
            schema,
            rdpIngressRules.Concat(sipIngressRules));
            app.Synth();
        }
    }
}
