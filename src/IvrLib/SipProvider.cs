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
        public IEnumerable<SecurityGroupRule> Select(string region, IEnumerable<Port> ingressPorts)
        {
            if(Rules.TryGetValue(region, out var rules)) {
                return rules.SelectMany(rule => {
                    var rules2 = new List<SecurityGroupRule>();
                    if(rule is EgressRule) {
                        // for each egress rule, create corresponding ingress - for all ports
                        rules2.Add(new EgressRule(rule.Peer, rule.Port, $"{Description} {rule.Description}/egress", rule.RemoteRule));
                        rules2.AddRange(ingressPorts.Select(p => new IngressRule(rule.Peer, p, $"{Description} {rule.Description}/+ingress", rule.RemoteRule)));
                    }
                    else if(rule is IngressRule) {
                        // explicitly define should be transferred as they are
                        rules2.Add(new IngressRule(rule.Peer, rule.Port, $"{Description} {rule.Description}/ingress", rule.RemoteRule));
                    }
                    else throw new SecurityGroupRuleException(rule);
                    return rules2;
                });
            }
            return new List<SecurityGroupRule>();
        }
        public bool NameMatchesAny(IEnumerable<string> listOfNames)
        {
            return 0 == listOfNames.Count() || default != listOfNames.FirstOrDefault(name => name.Equals(Name, StringComparison.CurrentCultureIgnoreCase));
        }

    }
}