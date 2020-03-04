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
        public IvrVpc Vpc { get; protected set; }
        public IvrStack(Construct scope, string stackId, IvrStackProps stackProps = null) : base(scope, stackId, stackProps)
        {
            // We'll start with brand new VPC
            Vpc = new IvrVpc(this, $"OneAndOnly");

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

            // We have props.HostsDomainName registered in advance

            // Create new Route53 zone           
            //var theZone = new PublicHostedZone(this, $"{stackId}_Zone", new PublicHostedZoneProps
            //{
            //    ZoneName = stackProps.HostsDomainName,
            //    Comment = "Created by CDK for existing domain",
            //});
            // or select existing created by registrar
            var theZone = HostedZone.FromLookup(this, $"{stackId}_Zone", new HostedZoneProviderProps
            {
                DomainName = stackProps.HostsDomainName,
                //Comment = "HostedZone created by Route53 Registrar",
            });

            // Finally - create our instances!
            var hosts = new List<Instance_>();
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

                    hosts.Add(new Instance_(this, $"Host_{instanceProps.PrivateIpAddress}".AsCloudFormationId(), instanceProps));
                }
            }
            // assign elastic IP to each host
            var eips = hosts.Select(h => {
                return new CfnEIP(this, $"EIP_{h.InstancePrivateIp}".AsCloudFormationId(), new CfnEIPProps
                {
                    Domain = "vpn",
                    InstanceId = h.InstanceId,
                });
            });
            // and register them all at once
            var ar = new ARecord(this, $"AR53".AsCloudFormationId(), new ARecordProps
            {
                Zone = theZone,
                RecordName = $"hosts.{theZone.ZoneName}",
                Target = RecordTarget.FromValues(eips.Select(eip => eip.Ref).ToArray()),
                Ttl = Duration.Seconds(300),
            });
        }
    }
}
