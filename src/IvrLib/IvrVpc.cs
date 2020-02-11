using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace IvrLib
{
    public class VpcProps : Amazon.CDK.AWS.EC2.VpcProps
    {
        public VpcProps()
        {
            Cidr = "10.0.0.0/24";
            MaxAzs = 2;
            SubnetConfiguration = new SubnetConfiguration[]{
                new SubnetConfiguration{
                    SubnetType = SubnetType.PUBLIC,
                    Name = "Ivr",
                    CidrMask = 26,
                },
            };
        }
    }

    public class Vpc :  Amazon.CDK.AWS.EC2.Vpc
    {
        public Vpc(Construct scope, string id = null, VpcProps props = null) : base(scope, id ?? "IvrVpc", props ?? new VpcProps())
        {

        }
    }

}