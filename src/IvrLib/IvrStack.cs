using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Route53;
//using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;

namespace IvrLib
{
    public class IvrStack : Stack
    {
        public IVpc Vpc { get; protected set; }
        public Instance_ Host { get; protected set; }
        public IvrStack(Construct scope, string id, IvrStackProps props = null) : base(scope, id, props)
        {
            // We'll start with brand new VPC
            this.Vpc = new Vpc(this, $"Public", new VpcProps
            {
                Cidr = "10.0.0.0/20",
                MaxAzs = 2,
                SubnetConfiguration = new SubnetConfiguration[] {
                    new SubnetConfiguration {
                        Name = "Public",
                        SubnetType = SubnetType.PUBLIC,
                        CidrMask = 24
                    },
                }
            });

            var role = new Role(this, "Role_CallHost", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
                InlinePolicies = new IvrInlinePolicies(props),
            });
            
            // Configure inbound security for RDP (and more?)
            var securityGroup = new SecurityGroup(this, $"Ingress", new SecurityGroupProps
            {
                Vpc = Vpc,
                AllowAllOutbound = props.AllowAllOutbound,
            });
            props.SecurityGroupRules.ForEach(rule => securityGroup.WithSecurityGroupRule(rule));

            var hostProps = new HostPrimingProps
            {
                HostName = $"{id}",
                AwsAccount = props.Env.Account,
                AwsRoleName = role.RoleName,
                RdpUserName = props.RdpUserName,
                RdpUserPassword = props.RdpUserPassword,
                RdpUserGroups = props.RdpUserGroups,
                EC2Users = props.EC2Users,
                S3iArgs = props.s3i_args,
            };

            // Finally - create our instance!
            var instanceProps = new InstanceProps
            {
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.LARGE),
                MachineImage = new WindowsImage(WindowsVersion.WINDOWS_SERVER_2019_ENGLISH_FULL_BASE),
                Vpc = Vpc,
                BlockDevices = new BlockDevice[] {
                    new BlockDevice {
                        DeviceName = "/dev/sda1",
                        Volume = BlockDeviceVolume.Ebs(30, new EbsDeviceOptions {
                            VolumeType = EbsDeviceVolumeType.STANDARD,
                            Encrypted = true,
                        }),
                    },
                },
                KeyName = props.KeyPairName,
                Role = role,
                SecurityGroup = securityGroup,
                VpcSubnets = new SubnetSelection
                {
                    SubnetType = SubnetType.PUBLIC
                },
                PrivateIpAddress = "10.0.0.4",
                UserData = HostPriming.PrimeForS3i(hostProps).UserData,
            };
            Host = new Instance_(this, $"CallHost", instanceProps);

            var eip = new CfnEIP(this, "IvrEIP", new CfnEIPProps { 
                Domain = "vpn",
                InstanceId = Host.InstanceId,
            });
            WriteLine($"EIP: lid:{eip.LogicalId}, iid:{eip.InstanceId}, pool:{eip.PublicIpv4Pool}, domain:{eip.Domain}");

//            // Add it to our DNS           
//            var publicZone = new PublicHostedZone(this, $"{id}_Zone", new PublicHostedZoneProps {
//               ZoneName = "host.ivrstack-au.net",
//                Comment = "Created by CDK for existing domain",
//            });

            var theZone = HostedZone.FromLookup(this, $"{id}_Zone", new HostedZoneProviderProps {
                DomainName = props.HostsDomainName,
            });

            // register the EIP... but HOW???
            new ARecord(this, $"{id}_Host_{instanceProps.PrivateIpAddress.Replace('.', '_')}", new ARecordProps {
                Zone = theZone,
                RecordName = $"trunk.{theZone.ZoneName}",
                Target = RecordTarget.FromIpAddresses(Host.InstancePublicIp),   // HOW ??
                Ttl = Duration.Seconds(300),
            });
/*
            // register Trunk private IP
            new ARecord(this, $"{id}_HostsPrivate", new ARecordProps {
                Zone = theZone,
                RecordName = $"trunk.{theZone.ZoneName}",
                Target = RecordTarget.FromIpAddresses(Host.InstancePrivateIp),
                Ttl = Duration.Seconds(300),
            });

            // register public IP
            new ARecord(this, $"{id}_Host_{instanceProps.PrivateIpAddress.Replace('.', '_')}", new ARecordProps {
                Zone = theZone,
                RecordName = $"{hostProps.HostName}.{theZone.ZoneName}",
                Target = RecordTarget.FromIpAddresses(Host.InstancePublicIp),
                Ttl = Duration.Seconds(300),
            });
            // register public IP for SIP DNS LB
            new ARecord(this, $"{id}_SIP_{instanceProps.PrivateIpAddress.Replace('.', '_')}", new ARecordProps {
                Zone = theZone,
                RecordName = $"sip.{theZone.ZoneName}",
                Target = RecordTarget.FromIpAddresses(Host.InstancePublicIp),
                Ttl = Duration.Seconds(300),
            });
*/
        }
    }
}
