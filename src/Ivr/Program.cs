using Amazon.CDK;
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
            var ctx = app.Node.WithJsonFiles($"{OSAgnostic.Home}/cdk.json", $"{Directory.GetCurrentDirectory()}/cdk.json");

            // default (public) context parameters
            var account = app.Node.TryGetContext("account") as string;
            if(string.IsNullOrWhiteSpace(account)) account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT");

            var region = app.Node.TryGetContext("region") as string;
            if(string.IsNullOrWhiteSpace(region)) region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION");
            System.Console.WriteLine($"Account/region: {account}/{region}");

            // mandatory (proprietary) context parameters
            var rdps = app.Node.TryGetContext("rdps") as string;
            if(string.IsNullOrWhiteSpace(rdps)) throw new ApplicationException("No RDP CIDR specified, use '-c rdps=<comma-separated_RDP_CIDRs>'");

            var keypair = app.Node.TryGetContext("keypair") as string;
            if(string.IsNullOrWhiteSpace(keypair)) throw new ApplicationException("No KeyPair specified, use '-c keypair=<name>'");

            var username = app.Node.TryGetContext("username") as string;
            if(string.IsNullOrWhiteSpace(username)) throw new ApplicationException("No UserName specified, use '-c username=<name>'");
            var password = app.Node.TryGetContext("password") as string;
            if(string.IsNullOrWhiteSpace(password)) throw new ApplicationException("No Password specified, use '-c password=<password>'");
            //throw new ApplicationException("ENOUGH");

            var ingress = new Dictionary<string, int>(rdps.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => new KeyValuePair<string, int>(x, 3389)));

            var ivrStack = new IvrStack(app, "IvrStack", new IvrStackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = account,
                    Region = region,
                },
                KeyName = keypair,
                UserName = username,
                UserPassword = password,
                IngressRules = ingress,
            });


            // do work
            app.Synth();
        }
    }
}
