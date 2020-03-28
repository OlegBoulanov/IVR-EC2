using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class IvrVpcProps
    {
        public string VpcCidrAddr { get; set; } = "10.0.0.0";
        public int VpcCidrMask { get; set; } = 24;  // 10.0.0.*
        public int SubnetCidrMask { get; set; } = 27;   // .0, .32, .64, .96, ...
        public int MaxAzs { get; set; } = 2;
        public const int MaxIpsPerSubnet = 251; // excluding 5 addresses: .0, .1, .2, .3,    .255
    }
}