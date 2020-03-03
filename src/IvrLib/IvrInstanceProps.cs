using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace IvrLib
{
    public class IvrInstanceProps
    {
        public static InstanceProps InstanceProps(IVpc vpc, ISubnet subnet, IRole role, ISecurityGroup securityGroup, string privateIpAddress = null)
        {
            var props = new InstanceProps
            {
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.LARGE),
                MachineImage = new WindowsImage(WindowsVersion.WINDOWS_SERVER_2019_ENGLISH_FULL_BASE),
                Vpc = vpc,
                BlockDevices = new BlockDevice[] {
                    new BlockDevice {
                        DeviceName = "/dev/sda1",
                        Volume = BlockDeviceVolume.Ebs(30, new EbsDeviceOptions {
                            VolumeType = EbsDeviceVolumeType.STANDARD,
                            Encrypted = true,
                        }),
                    },
                },
                Role = role,
                SecurityGroup = securityGroup,
                VpcSubnets = new SubnetSelection
                {
                    //SubnetType = SubnetType.PUBLIC,
                    //SubnetGroupName = subnetGroupName,
                    Subnets = new ISubnet[] { subnet },
                },
            };        
            if(!string.IsNullOrWhiteSpace(privateIpAddress)){
                props.PrivateIpAddress = privateIpAddress;
            }
            return props;
        }
    }
}