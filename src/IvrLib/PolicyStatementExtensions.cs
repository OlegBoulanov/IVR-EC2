using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace IvrLib
{
    public static class PolicyStatementExtensions
    {
        public static PolicyStatement WithEffect(this PolicyStatement statement, Effect effect)
        {
            statement.Effect = effect;
            return statement;
        }
        public static PolicyStatement Allow(this PolicyStatement statement)
        {
            return statement.WithEffect(Effect.ALLOW);
        }
        public static PolicyStatement Deny(this PolicyStatement statement)
        {
            return statement.WithEffect(Effect.DENY);
        }
        public static PolicyStatement WithActions(this PolicyStatement statement, params string[] actions)
        {
            statement.AddActions(actions);
            return statement;
        }
        public static PolicyStatement WithNotActions(this PolicyStatement statement, params string[] actions)
        {
            statement.AddNotActions(actions);
            return statement;
        }
        public static PolicyStatement WithResources(this PolicyStatement statement, params string[] resources)
        {
            statement.AddResources(resources);
            return statement;
        }    
        public static PolicyStatement WithNotResources(this PolicyStatement statement, params string[] resources)
        {
            statement.AddNotResources(resources);
            return statement;
        }    
    }
}