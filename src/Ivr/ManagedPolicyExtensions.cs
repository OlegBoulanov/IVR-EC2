using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace Ivr
{
    public static class ManagedPolicyExtensions
    {
        public static ManagedPolicy WithStatements(this ManagedPolicy policy, params PolicyStatement[] statements)
        {
            policy.AddStatements(statements);
            return policy;
        } 
    }
}