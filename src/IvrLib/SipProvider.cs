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
        public IEnumerable<SecurityGroupRule> Select(string region, IEnumerable<IngressPort> ingressPorts)
        {
            if (Rules.TryGetValue(region, out var rules))
            {
                return rules.SelectMany(rule =>
                {
                    var rules2 = new List<SecurityGroupRule>();
                    if (rule is IngressRuleTemplate)
                    {
                        // ingress rule template translates to several rules, thus opening all specified ingress ports
                        rules2.AddRange(ingressPorts.Select(p => new IngressRule(rule.Peer, p.Port, $"{Description} {rule.Description ?? "Ingress"}:{p.Description ?? "*"}", rule.RemoteRule)));
                    }
                    else if (rule is IngressRule)
                    {
                        // explicitly defined ingress
                        rules2.Add(new IngressRule(rule.Peer, rule.Port, $"{Description} {rule.Description}/ingress", rule.RemoteRule));
                    }
                    else if (rule is EgressRule)
                    {
                        // egress are always explicit
                        rules2.Add(new EgressRule(rule.Peer, rule.Port, $"{Description} {rule.Description}/egress", rule.RemoteRule));
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