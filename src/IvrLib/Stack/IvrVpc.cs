using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class IvrVpc : Vpc
    {
        public IvrVpc(Construct scope, string id, IvrVpcProps props)
        : base(scope, id, new VpcProps {
            Cidr = $"{props.VpcCidrAddr}/{props.VpcCidrMask}",
            MaxAzs = props.MaxAzs,
            EnableDnsHostnames = false,
            EnableDnsSupport = true,
            SubnetConfiguration = new SubnetConfiguration[]
            {
                   new SubnetConfiguration {
                    Name = "Public",
                    SubnetType = SubnetType.PUBLIC,
                    CidrMask = props.SubnetCidrMask,
                },
            }
        }) {}
    }
}
