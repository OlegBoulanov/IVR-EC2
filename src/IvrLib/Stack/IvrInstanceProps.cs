using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace IvrLib
{
    public class IvrInstanceProps
    {
        public InstanceClass InstanceClass { get; set; } = InstanceClass.BURSTABLE3;
        public InstanceSize InstanceSize { get; set; } = InstanceSize.LARGE;
        public WindowsVersion WindowsVersion { get; set; } = WindowsVersion.WINDOWS_SERVER_2019_ENGLISH_FULL_BASE;
        public double VolumeSize { get; set; } = 30;
        public EbsDeviceVolumeType VolumeType { get; set; } = EbsDeviceVolumeType.STANDARD;
        public static InstanceProps InstanceProps(IVpc vpc, ISubnet subnet, IRole role, ISecurityGroup securityGroup, IvrInstanceProps props, string privateIpAddress = null)
        {
            var instanceProps = new InstanceProps
            {
                InstanceType = InstanceType.Of(props.InstanceClass, props.InstanceSize),
                MachineImage = new WindowsImage(props.WindowsVersion),
                Vpc = vpc,
                BlockDevices = new BlockDevice[] {
                    new BlockDevice {
                        DeviceName = "/dev/sda1",
                        Volume = BlockDeviceVolume.Ebs(props.VolumeSize, new EbsDeviceOptions {
                            VolumeType = props.VolumeType,
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
                instanceProps.PrivateIpAddress = privateIpAddress;
            }
            return instanceProps;
        }
    }
}