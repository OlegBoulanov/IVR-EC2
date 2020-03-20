using System;
using System.IO;
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

using IvrLib.Security;
using IvrLib.Utils;

namespace IvrLib
{
    public class IvrStack : Stack
    {
        public IvrStack(Construct scope, string stackId, StackProps stackProps, IvrSiteSchema schema, IEnumerable<SecurityGroupRule> securityGroupRules) : base(scope, stackId, stackProps)
        {
            // We'll start with brand new VPC
            var vpc = new IvrVpc(this, $"OneAndOnly_", schema.VpcProps);

            var s3gwep = new GatewayVpcEndpoint(this, $"S3GW_", new GatewayVpcEndpointProps { 
                Vpc = vpc,
                Service = GatewayVpcEndpointAwsService.S3, 
                Subnets = new SubnetSelection[] { new SubnetSelection { SubnetType = SubnetType.PUBLIC, } },
            });

            var iamRole = new Role(this, "Role_CallHost_", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
                InlinePolicies = new IvrInlinePolicies(stackProps.Env.Account, stackId, schema),
            });

            // Configure inbound security for RDP (and more?)
            var securityGroup = new SecurityGroup(this, $"Ingress_", new SecurityGroupProps
            {
                Vpc = vpc,
                AllowAllOutbound = schema.AllowAllOutbound,
            });
            securityGroupRules.ForEach(rule => securityGroup.WithSecurityGroupRule(rule));

            // Finally - create our instances!
            var hosts = new List<HostInstance>();
            for(var subnetIndex = 0; ++subnetIndex <= vpc.PublicSubnets.Length; )
            {
                var instanceProps = IvrInstanceProps.InstanceProps(vpc, vpc.PublicSubnets[subnetIndex - 1], iamRole, securityGroup, privateIpAddress: null);
                var hostIndexInSubnet = 0;
                foreach(var group in schema.HostGroups)
                {
                    for (var hostCount = 0; ++hostCount <= Math.Min(group.HostCount, IvrVpcProps.MaxIpsPerSubnet); )
                    {
                        var hostName = $"{schema.HostNamePrefix}{subnetIndex}{++hostIndexInSubnet:00}";
                        var hostPrimingProps = new HostPrimingProps
                        {
                            HostName = hostName.AsWindowsComputerName(),   // must fit into 15 chars
                            WorkingFolder = $"CDK-{stackId}".AsWindowsFolder(),
                            AwsAccount = stackProps.Env.Account,
                            AwsRoleName = iamRole.RoleName,
                            RdpUserName = schema.RdpProps.UserName,
                            RdpUserPassword = schema.RdpProps.Password,
                            RdpUserGroups = schema.RdpProps.UserGroups,
                            EC2Users = schema.EC2Users,
                            S3iArgs = $"{group.InstallFrom} --verbose", 
                        };
                        instanceProps.KeyName = schema.KeyPairName;
                        instanceProps.UserData = HostPriming.PrimeForS3i(hostPrimingProps).UserData;
                        hosts.Add(new HostInstance 
                        { 
                            Group = group, 
                            Instance = new Instance_(this, hostName.AsCloudFormationId(), instanceProps), 
                        });
                    }
                }
            }
            // Create new Route53 zone           
            //var theZone = new PublicHostedZone(this, $"{stackId}_Zone", new PublicHostedZoneProps
            //{
            //    ZoneName = stackProps.HostsDomainName,
            //    Comment = "Created by CDK for existing domain",
            //});

            // or select existing created by registrar
            // We have schema.Domain registered in advance
            var theZone = HostedZone.FromLookup(this, $"{stackId}_Zone_", new HostedZoneProviderProps
            {
                DomainName = schema.HostedZoneDomain,
                //Comment = "HostedZone created by Route53 Registrar",
            });
            // assign Elastic IPs as needed
            var eips = hosts.Where(h => h.Group.UseElasticIP).Select(h => {
                return new CfnEIP(this, $"EIP_{h.Instance.InstancePrivateIp}_".AsCloudFormationId(), new CfnEIPProps
                {
                    Domain = "vpc", // 'standard' or 'vpc': https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/aws-properties-ec2-eip.html#cfn-ec2-eip-domain
                    InstanceId = h.Instance.InstanceId,
                });
            });
            // register Elastic IPs
            var arPublic = new ARecord(this, $"ARecord_Public_".AsCloudFormationId(), new ARecordProps
            {
                Zone = theZone,
                RecordName = $"{schema.SubdomainPublic}.{theZone.ZoneName}",
                Target = RecordTarget.FromValues(eips.Select(eip => eip.Ref).ToArray()),
                Ttl = Duration.Seconds(300),
            });
            // and private hosts
            var arPrivate = new ARecord(this, $"ARecord_Private_".AsCloudFormationId(), new ARecordProps
            {
                Zone = theZone,
                RecordName = $"{schema.SubdomainPrivate}.{theZone.ZoneName}",
                Target = RecordTarget.FromIpAddresses(hosts.Select(h => h.Instance.InstancePrivateIp).ToArray()),
                Ttl = Duration.Seconds(300),
            });
        }
    }
}
