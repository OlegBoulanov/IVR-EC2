using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class IvrVpc : Vpc
    {
        public IvrVpc(Construct scope, string id)
        : base(scope, id, new VpcProps {
            Cidr = "10.0.0.0/16",
            EnableDnsHostnames = false,
            EnableDnsSupport = true,
            MaxAzs = 2,
            SubnetConfiguration = new SubnetConfiguration[]
            {
                   new SubnetConfiguration {
                    Name = "Public",
                    SubnetType = SubnetType.PUBLIC,
                    CidrMask = 24,
                },
            }
        })
        {
        
        }
    }
}
