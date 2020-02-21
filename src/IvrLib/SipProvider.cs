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
    }
}