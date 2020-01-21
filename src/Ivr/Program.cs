using Amazon.CDK;
using System;
using System.IO;
using static System.Console;
using System.Collections.Generic;
using System.Linq;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivr
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var userProfile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            var userCdkJsonFile = $"{userProfile}{Path.DirectorySeparatorChar}cdk.json";
            try
            {
                using (var stream = new FileStream(userCdkJsonFile, FileMode.Open))
                {
                    var jd = JsonDocument.Parse(stream);

                    var e = jd.RootElement.Select("context", "RdpCidr", "Public");
                    
                }
            }
            catch(FileNotFoundException)
            { 
                // no special user settings
            }
            catch(JsonException x)
            {
                throw new FileLoadException("Error parsing json", userCdkJsonFile, x);
            }
            catch (InvalidOperationException x)
            {
                throw new FileLoadException(x.Message, userCdkJsonFile, x);
            }
            catch (Exception x)
            {
                throw new FileLoadException($"{x.GetType().Name}: {x.Message}", userCdkJsonFile, x);
            }

            //return;
            var account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT");
            var region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION");
            System.Console.WriteLine($"Main(): CdkAcc: {account}/{region}");

            var rdpPublicCidr = System.Environment .GetEnvironmentVariable("RDP_PUBLIC_CIDR");
            if(string.IsNullOrWhiteSpace(rdpPublicCidr)) {
                WriteLine($"Main(): RDP_PUBLIC_IP is not set");
            }            

            var app = new App();
            var ivrStack = new IvrStack(app, "IvrStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = account,
                    Region = region,
                }
            });
            ivrStack.RdpPublicCidr = rdpPublicCidr;
            // now run it again
            app.Synth();
        }
    }
}
