using System;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace IvrLib
{
    public static class SecurityGroupExtensions
    {
        public static SecurityGroup WithSecurityGroupRule(this SecurityGroup group, SecurityGroupRule rule)
        {
            if(rule is IngressRule) group.AddIngressRule(rule.Peer, rule.Port, rule.Description, rule.RemoteRule);
            else if(rule is EgressRule) group.AddEgressRule(rule.Peer, rule.Port, rule.Description, rule.RemoteRule);
            else throw new SecurityGroupRuleException(rule);
            return group;
        }
    }
}