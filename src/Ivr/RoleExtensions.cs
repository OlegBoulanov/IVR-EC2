using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace Ivr
{
    public static class MRoleExtensions
    {
        public static RoleProps SetManagedPolicies(this RoleProps role, params IManagedPolicy[] policies)
        {
            role.ManagedPolicies = policies;
            return role;
        } 
    }
}