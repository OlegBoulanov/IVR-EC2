using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class IvrVpcProps
    {
        public string VpcCidrAddr { get; set; } = "10.0.0.0";
        public int VpcCidrMask { get; set; } = 16;  // class B
        public int SubnetCidrMask { get; set; } = 24;   // class C 
        public int MaxAzs { get; set; } = 2;
        public const int MaxIpsPerSubnet = 251; // excluding 0, 1, 2, 3, 255
    }
}