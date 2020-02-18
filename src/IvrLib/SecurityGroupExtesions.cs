using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace IvrLib
{
    public static class SecurityGroupExtensions
    {
        public static SecurityGroup WithIngressRule(this SecurityGroup group, IngressRuleProps props)
        {
            group.AddIngressRule(props.Peer, props.Connection, props.Description, props.RemoteRule);
            return group;
        }
        public static SecurityGroup WithIngressRule(this SecurityGroup group, IngressRule rule)
        {
            return group.WithIngressRule(rule.Props);
        }
    }
}