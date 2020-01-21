using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ivr
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT");
            var region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION");
            System.Console.WriteLine($"Main(): CdkAcc: {account}/{region}");

            var app = new App();
            new IvrStack(app, "IvrStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = account,
                    Region = region
                }
            });
            // now run it again
            app.Synth();
        }
    }
}
