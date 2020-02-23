using System;

namespace IvrLib
{
    public class SecurityGroupRuleException : ApplicationException
    {
        public SecurityGroupRuleException(SecurityGroupRule rule) : base($"SecurityGroup rule {rule.GetType().Name} ({rule.Peer}, {rule.Port}: {rule.Description}) is not expected")
        {
        }
    }
}