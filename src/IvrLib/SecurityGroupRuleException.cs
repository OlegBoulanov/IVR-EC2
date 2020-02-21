using System;

namespace IvrLib
{
    public class SecurityGroupRuleException : ApplicationException
    {
        public SecurityGroupRuleException(SecurityGroupRule rule) : base($"SecurityGroup rule ({rule.Peer}, {rule.Port}: {rule.Description}) is not Ingress or Egress")
        {
        }
    }
}