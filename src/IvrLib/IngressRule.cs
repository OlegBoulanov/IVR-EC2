using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace IvrLib
{
    public class IngressRule
    {
        public IngressRuleProps Props { get; set; }
        public IngressRule(IngressRuleProps props = null)
        {
            Props = props;
        }
    }
}
