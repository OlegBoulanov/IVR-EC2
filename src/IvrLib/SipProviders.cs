using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class SipProviders
    {
        public static IEnumerable<SecurityGroupRule> Select(string region, IEnumerable<string> providers, IEnumerable<PortSpec> ingressPorts)
        {
            return Providers.Aggregate(new List<SecurityGroupRule>(), (list, provider) =>
            {
                if (null == providers || provider.NameMatchesAny(providers))
                {
                    list.AddRange(provider.Select(region, ingressPorts));
                }
                return list;
            });
        }
        protected static IEnumerable<SipProvider> Providers = new List<SipProvider>
        {
            new AmazonChimeVoiceConnector(),
            new TwilioElasticSipTrunking(),
            new PlivoZenTrunk(),
        };
    }
}