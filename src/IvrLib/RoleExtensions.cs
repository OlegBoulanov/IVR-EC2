using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace IvrLib
{
    public static class RoleExtensions
    {
        public static RoleProps SetManagedPolicies(this RoleProps role, params IManagedPolicy[] policies)
        {
            role.ManagedPolicies = policies;
            return role;
        } 
    }
}