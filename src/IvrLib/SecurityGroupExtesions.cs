using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace IvrLib
{
    public static class SecurityGroupExtensions
    {
        public static SecurityGroup WithIngressRule(this SecurityGroup group, IngressRule rule)
        {
            group.AddIngressRule(rule.Peer, rule.Port, rule.Description, rule.RemoteRule);
            return group;
        }
    }
}