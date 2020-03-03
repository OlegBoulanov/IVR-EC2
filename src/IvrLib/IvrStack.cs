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
        public IvrStack(Construct scope, string stackId, IvrStackProps stackProps = null) : base(scope, stackId, stackProps)
        {
            // We'll start with brand new VPC
            this.Vpc = new Vpc(this, $"VPC", new VpcProps
            {
                Cidr = "10.0.0.0/16",
                EnableDnsHostnames = false,
                EnableDnsSupport = true,
                MaxAzs = 2,
                SubnetConfiguration = new SubnetConfiguration[] {
                    new SubnetConfiguration {
                        Name = "Public",
                        SubnetType = SubnetType.PUBLIC,
                        CidrMask = 24,
                    },
                }
            });

            var role = new Role(this, "Role_CallHost", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
                InlinePolicies = new IvrInlinePolicies(stackProps),
            });

            // Configure inbound security for RDP (and more?)
            var securityGroup = new SecurityGroup(this, $"Ingress", new SecurityGroupProps
            {
                Vpc = Vpc,
                AllowAllOutbound = stackProps.AllowAllOutbound,
            });
            stackProps.SecurityGroupRules.ForEach(rule => securityGroup.WithSecurityGroupRule(rule));

            // Finally - create our instances!
            var subnetIpFormats = new string[] { "10.0.0.{0}", "10.0.1.{0}" };
            for(var subnetIndex = 0; subnetIndex < subnetIpFormats.Length; ++subnetIndex)
            {
                var subnetIpFormat = subnetIpFormats[subnetIndex];
                for (var subnetIpIndex = 0; subnetIpIndex < Math.Min(2, 251); ++subnetIpIndex)
                {
                    var privateIpAddress = string.Format(subnetIpFormat, 4 + subnetIpIndex);
                    var instanceProps = IvrInstanceProps.InstanceProps(Vpc, Vpc.PublicSubnets[subnetIndex], role, securityGroup, privateIpAddress);
                    var hostPrimingProps = new HostPrimingProps
                    {
                        HostName = $"CH-{privateIpAddress.Substring(5)}".AsWindowsComputerName(),   // must fit into 15 chars
                        WorkingFolder = $"{stackId}".AsWindowsFolder(),
                        AwsAccount = stackProps.Env.Account,
                        AwsRoleName = role.RoleName,
                        RdpUserName = stackProps.RdpUserName,
                        RdpUserPassword = stackProps.RdpUserPassword,
                        RdpUserGroups = stackProps.RdpUserGroups,
                        EC2Users = stackProps.EC2Users,
                        S3iArgs = stackProps.s3i_args,
                    };
                    instanceProps.KeyName = stackProps.KeyPairName;
                    instanceProps.UserData = HostPriming.PrimeForS3i(hostPrimingProps).UserData;
                    Host = new Instance_(this, $"Host_{instanceProps.PrivateIpAddress}".AsCloudFormationId(), instanceProps);

                    var eip = new CfnEIP(this, $"EIP_{instanceProps.PrivateIpAddress}".AsCloudFormationId(), new CfnEIPProps
                    {
                        Domain = "vpn",
                        InstanceId = Host.InstanceId,
                    });
                    WriteLine($"EIP: lid:{eip.LogicalId}, iid:{eip.InstanceId}, pool:{eip.PublicIpv4Pool}, domain:{eip.Domain}");
                }
            }

            //            // Add it to our DNS           
            //            var publicZone = new PublicHostedZone(this, $"{id}_Zone", new PublicHostedZoneProps {
            //               ZoneName = "host.ivrstack-au.net",
            //                Comment = "Created by CDK for existing domain",
            //            });
            /*
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
