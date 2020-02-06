using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;

namespace Ivr
{
    public class IvrStack : Stack
    {
        public IVpc Vpc { get; protected set; }
        public Instance_ Instance { get; protected set; }
        public UserData UserData { get; protected set; }
        internal IvrStack(Construct scope, string id, IvrStackProps props = null) : base(scope, id, props)
        {
            this.Vpc = new Ivr.Vpc(this, $"{id}_Vpc", new VpcProps
            {
                Cidr = "10.10.10.0/24",
                MaxAzs = 2,
                SubnetConfiguration = new SubnetConfiguration[] {
                    new SubnetConfiguration {
                        Name = "Public",
                        SubnetType = SubnetType.PUBLIC,
                        CidrMask = 26
                    }
                }
            });

            WriteLine($"VPC: {Vpc.VpcId}/{Vpc.VpcCidrBlock}");

            var publicSubnets = Vpc.PublicSubnets;
            WriteLine($"{publicSubnets.Aggregate($"{nameof(IvrStack)}.PublicSubnets[{publicSubnets.Length}]:", (a, subnet) => { return $"{a}{System.Environment.NewLine}  {subnet.SubnetId}/{subnet.AvailabilityZone} => {subnet.RouteTable.RouteTableId}"; })}");

            var amiImage = new WindowsImage(WindowsVersion.WINDOWS_SERVER_2019_ENGLISH_FULL_BASE);
            var amiImageConfig = amiImage.GetImage(this);
            WriteLine($"Win: {amiImageConfig.OsType}/{amiImageConfig.ImageId}");

            var role = new Role(this, "IvrRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
            });
            //role.AddManagedPolicy(new IvrPolicy(this, $"{id}_Policy"));
            WriteLine($"Role: {role.RoleArn}");

            var securityGroup = new SecurityGroup(this, $"{id}_SG", new SecurityGroupProps
            {
                Vpc = Vpc,
                AllowAllOutbound = true,
            });
            // add IB RDP 
            foreach(var rule in props.IngressRules)
            {
                securityGroup.AddIngressRule(Peer.Ipv4(rule.Key), Port.Tcp(rule.Value), $"Inbound: {rule.Key}:{rule.Value}");    
            }

//            var eip = new CfnEIP(this, "IvrEIP", new CfnEIPProps
//            {
//            });
//            WriteLine($"EIP: {eip.LogicalId}");

            this.Instance = new Instance_(this, $"{id}_Instance", new InstanceProps
            {
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.SMALL),
                
                MachineImage = amiImage,
                Vpc = Vpc,
                BlockDevices = new BlockDevice[]
                {
                    new BlockDevice
                    {
                        DeviceName = "/dev/sda1",
                        Volume = BlockDeviceVolume.Ebs(30, new EbsDeviceOptions
                        {
                            VolumeType = EbsDeviceVolumeType.STANDARD,
                            Encrypted = true,
                        }),
                    },
                },
                
                KeyName = props.KeyName,
                Role = role,
                SecurityGroup = securityGroup,
                VpcSubnets = new SubnetSelection
                {
                    SubnetType = SubnetType.PUBLIC
                },
                //UserData = UserData,
            });
        }
    }
}
