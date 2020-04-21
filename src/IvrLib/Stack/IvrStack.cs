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
            IVpc vpc = null;
            if (!string.IsNullOrWhiteSpace(schema.VpcName))
            {
                vpc = Vpc.FromLookup(this, "$VPC", new VpcLookupOptions { VpcName = schema.VpcName, }); // will error if not found
            }
            else if (!string.IsNullOrWhiteSpace(schema.VpcId))
            {
                vpc = Vpc.FromLookup(this, "$VPC", new VpcLookupOptions { VpcId = schema.VpcId, }); // will error if not found
            }
            else if(null != schema.VpcProps)
            {
                // use provided props to create brand new VPC
                vpc = new IvrVpc(this, $"VPC", schema.VpcProps);
            }

            if (schema.AddVpcS3Gateway)
            {
                var s3gw = new GatewayVpcEndpoint(this, $"S3GW", new GatewayVpcEndpointProps
                {
                    Vpc = vpc,
                    Service = GatewayVpcEndpointAwsService.S3,
                    Subnets = new SubnetSelection[] { new SubnetSelection { SubnetType = SubnetType.PUBLIC, } },
                });
            }

            var role = new Role(this, "IVR", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
                InlinePolicies = new IvrInlinePolicies(stackProps.Env.Account, stackId, schema),
            });

            // Configure inbound security for RDP (and more?)
            var securityGroup = new SecurityGroup(this, $"Ingress", new SecurityGroupProps
            {
                Vpc = vpc,
                AllowAllOutbound = schema.AllowAllOutbound,
            });
            securityGroupRules.ForEach(rule => securityGroup.WithSecurityGroupRule(rule));
            if(schema.AllowAllIntranet) securityGroup.WithSecurityGroupRule(new IngressRule(Peer.Ipv4($"{vpc.VpcCidrBlock}"), Port.AllTraffic()).WithDescription($"All intranet traffic"));

            // Finally - create our instances!
            var hosts = new List<HostInstance>();
            for(var subnetIndex = 0; ++subnetIndex <= vpc.PublicSubnets.Length; )
            {
                var hostIndexInSubnet = 0;
                foreach(var group in schema.HostGroups)
                {
                    var numberOfHosts = Math.Min(group.HostCount, IvrVpcProps.MaxIpsPerSubnet);
                    if(numberOfHosts != group.HostCount) {
                        Console.WriteLine($"Group({group.Name}) host count changed from {group.HostCount} to {numberOfHosts}");
                        group.HostCount = numberOfHosts;
                    }
                    var instanceProps = IvrInstanceProps.InstanceProps(vpc, vpc.PublicSubnets[subnetIndex - 1], role, securityGroup, group.InstanceProps);
                    for (var hostCount = 0; ++hostCount <= numberOfHosts; ++hostIndexInSubnet)
                    {
                        var hostName = $"{schema.HostNamePrefix}{subnetIndex}{hostIndexInSubnet:00}";
                        var hostPrimingProps = new HostPrimingProps
                        {
                            HostName = hostName.AsWindowsComputerName(),   // must fit into 15 chars
                            WorkingFolder = $"{stackId}".AsWindowsFolder(),
                            AwsAccount = stackProps.Env.Account,
                            AwsRoleName = role.RoleName,
                            RdpProps = schema.RdpProps,
                            EC2Users = schema.EC2Users,
                            DownloadAndInstall = group.DownloadAndInstall,
                            S3iArgs = $"{group.InstallS3i} --verbose", 
                        };
                        var hostCommands = HostPriming.PrimeForS3i(hostPrimingProps)
                            .WithFirewallAllowInbound($"{vpc.VpcCidrBlock}");
                        hostCommands.WithRenameAndRestart(hostPrimingProps.HostName);
                        instanceProps.KeyName = schema.KeyPairName;
                        instanceProps.UserData = hostCommands.UserData;
                        hosts.Add(new HostInstance 
                        { 
                            Group = group, 
                            Instance = new Instance_(this, hostName.AsCloudFormationId(), instanceProps), 
                        });
                    }
                }
            }
            // associate pre-allocated EIPs
            var preAllocatedEIPs = schema.PreAllocatedElasticIPs.SelectMany(s => s.Csv());
            var hostsThatRequireEIP = hosts.Where(h => h.Group.UsePreAllocatedElasticIPs);
            if(preAllocatedEIPs.Count() < hostsThatRequireEIP.Count())
            {
                throw new ArgumentException($"Pre-Allocated Elastic IPs needed: {hostsThatRequireEIP.Count()}, but only {preAllocatedEIPs.Count()} configured in schema.{nameof(IvrSiteSchema.PreAllocatedElasticIPs)}");
            }
            var elasticIPAssociations = hostsThatRequireEIP.Zip(preAllocatedEIPs, (h, a) =>
            {
                return new CfnEIPAssociation(this, $"EIPA{h.Instance.InstancePrivateIp}".AsCloudFormationId(), new CfnEIPAssociationProps
                {
                    AllocationId = a,
                    InstanceId = h.Instance.InstanceId,
                });
            }).ToList();    // execute LINQ now
            foreach(var a in elasticIPAssociations) {
                Console.WriteLine($"Pre-Allocated Elastic IP Associations: {a.AllocationId}/{a.InstanceId}");
            }
            // Create new Route53 zone           
            //var theZone = new PublicHostedZone(this, $"{stackId}_Zone", new PublicHostedZoneProps
            //{
            //    ZoneName = stackProps.HostsDomainName,
            //    Comment = "Created by CDK for existing domain",
            //});

            // or select existing created by registrar
            // We have schema.Domain registered in advance
            if (!string.IsNullOrWhiteSpace(schema.HostedZoneDomain))
            {
                var theZone = HostedZone.FromLookup(this, $"{stackId}_Zone_", new HostedZoneProviderProps
                {
                    DomainName = schema.HostedZoneDomain,
                    //Comment = "HostedZone created by Route53 Registrar",
                });
                // assign new Elastic IPs as needed
                if (!string.IsNullOrWhiteSpace(schema.SubdomainEIPs))
                {
                    var newElasticIPs = hosts.Where(h => h.Group.AllocateNewElasticIPs).Select(h =>
                    {
                        return new CfnEIP(this, $"EIP{h.Instance.InstancePrivateIp}".AsCloudFormationId(), new CfnEIPProps
                        {
                            // 'standard' or 'vpc': https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/aws-properties-ec2-eip.html#cfn-ec2-eip-domain
                            Domain = "vpc",
                            InstanceId = h.Instance.InstanceId,
                        });
                    }).ToList();   // collect them now to prevent LINQ Count side effects
                    if (0 < newElasticIPs.Count)
                    {
                        // register (permanent) Elastic IPs
                        var arPublic = new ARecord(this, $"ARecord_Public_NewAlloc".AsCloudFormationId(), new ARecordProps
                        {
                            Zone = theZone,
                            RecordName = $"{schema.SubdomainEIPs}.{theZone.ZoneName}",
                            Target = RecordTarget.FromValues(newElasticIPs.Select(eip => eip.Ref).ToArray()),
                            Ttl = Duration.Seconds(300),
                        });
                    }
                }
                if(0 < hosts.Count && !string.IsNullOrWhiteSpace(schema.SubdomainHosts))
                {
                    // and private (never changing, as opposed to public - which change on stop/start) addresses of all hosts
                    var arPrivate = new ARecord(this, $"ARecord_Private_".AsCloudFormationId(), new ARecordProps
                    {
                        Zone = theZone,
                        RecordName = $"{schema.SubdomainHosts}.{theZone.ZoneName}",
                        Target = RecordTarget.FromIpAddresses(hosts.Select(h => h.Instance.InstancePrivateIp).ToArray()),
                        Ttl = Duration.Seconds(300),
                    });
                }
                //throw new Exception();
            }
        }
    }
}
