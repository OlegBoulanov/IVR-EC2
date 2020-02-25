using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class SipProvider
    {
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public IDictionary<string, IEnumerable<SecurityGroupRule>> Rules { get; protected set; }
        public SipProvider(string name, string description, IDictionary<string, IEnumerable<SecurityGroupRule>> rules)
        {
            Name = name;
            Description = description;
            Rules = rules;
        }
        public IEnumerable<SecurityGroupRule> Select(string region, IEnumerable<PortSpec> ingressPorts)
        {
            if (Rules.TryGetValue(region, out var rules))
            {
                return rules.SelectMany(rule =>
                {
                    var rules2 = new List<SecurityGroupRule>();
                    if (rule is IngressRule)
                    {
                        // explicitly defined ingress
                        rules2.Add(new IngressRule(rule.Peer, rule.Port, rule.Protocols).WithDescription($"{Description} {rule.Description}/ingress").WithRemoteRule(rule.RemoteRule));
                    }
                    else if (rule is EgressRule)
                    {
                        // egress are always explicit
                        rules2.Add(new EgressRule(rule.Peer, rule.Port, rule.Protocols).WithDescription($"{Description} {rule.Description}/egress").WithRemoteRule(rule.RemoteRule));
                        // add ingress rules for each port of the same protocol
                        rules2.AddRange(ingressPorts.Where(ip => rule.Protocols.Contains(ip.Protocol)).Select(ip => {
                            return new IngressRule(rule.Peer, rule.Port.Clone(ip.StartPort, ip.EndPort), rule.Protocols).WithDescription($"{Description} {rule.Description??ip.Protocol}/+ingress").WithRemoteRule(rule.RemoteRule);
                        }));
                    }
                    else throw new SecurityGroupRuleException(rule);
                    return rules2;
                });
            }
            return new List<SecurityGroupRule>();
        }
        public bool NameMatchesAny(IEnumerable<string> tags)
        {
            return 0 == tags.Count() || default != tags.FirstOrDefault(name => name.Equals(Name, StringComparison.CurrentCultureIgnoreCase));
        }

    }
}