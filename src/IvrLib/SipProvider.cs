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
        public IDictionary<string, IEnumerable<IngressRule>> Rules { get; protected set; }
        public SipProvider(string name, string description, IDictionary<string, IEnumerable<IngressRule>> rules)
        {
            Name = name;
            Description = description;
            Rules = rules;
        }
        protected string MakeDescription(IngressRule rule)
        {
            return $"{Description}{(string.IsNullOrWhiteSpace(rule.Description)?"":$" {rule.Description}")}";
        }
        public IEnumerable<IngressRule> Select(string region)
        {
            return Rules.TryGetValue(region, out var rules)? rules.Select(rule => new IngressRule(rule.Peer, rule.Port, MakeDescription(rule))) : new List<IngressRule>();
        }
    }
}