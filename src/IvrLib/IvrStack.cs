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

namespace IvrLib
{
    public class IvrStack : Stack
    {
        public IVpc Vpc { get; protected set; }
        public Instance_ Instance { get; protected set; }
        public IvrStack(Construct scope, string id, IvrStackProps props = null) : base(scope, id, props)
        {
            // We'll start with brand new VPC
            this.Vpc = new IvrLib.Vpc(this, $"Public", new VpcProps
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

            // Role is needed for allowing tools to use EC2 provided credentials
            // see https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-role.html
            var role = new Role(this, "Role_CallHost", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
                InlinePolicies = new Dictionary<string, PolicyDocument> {
                    { "IvrPolicy", new PolicyDocument(new PolicyDocumentProps {
                        Statements = new PolicyStatement[] {
                            new PolicyStatement().Allow().WithActions("sts:AssumeRole")
                                .WithResources($"arn:aws:iam::{props.Env.Account}:role/IvrStack*"), 
                            new PolicyStatement().Allow().WithActions("s3:GetBucketLocation")
                                .WithResources(),
                            new PolicyStatement().Allow().WithActions("s3:ListBucket")
                                .WithResources(props.S3BucketResources("apps", "config", "install", "prompts", "prompts.update", "tools", "userjobs")),
                            new PolicyStatement().Allow().WithActions("s3:GetObject")
                                .WithResources(props.S3ObjectResources("apps", "config", "install", "logs", "prompts", "prompts.update", "sessions", "segments", "tools", "userjobs")),
                            new PolicyStatement().Allow().WithActions("s3:PutObject")
                                .WithResources(props.S3ObjectResources("logs", "sessions", "segments", "tools")),
                            new PolicyStatement().Allow().WithActions("s3:DeleteObject")
                                .WithResources(props.S3ObjectResources("userjobs")),
                            new PolicyStatement().Allow().WithActions("sqs:DeleteMessage", "sqs:GetQueueAttributes", "sqs:GetQueueUrl", "sqs:ReceiveMessage", "sqs:SendMessage")
                                .WithResources(),
                            new PolicyStatement().Allow().WithActions("cloudwatch:GetMetricData", "cloudwatch:GetMetricStatistics", "cloudwatch:ListMetrics", "cloudwatch:PutMetricData")
                                .WithResources(),
                            new PolicyStatement().Allow().WithActions("sns:Publish")
                                .WithResources(),
                            new PolicyStatement().Allow().WithActions("ses:SendEmail")
                                .WithResources(),
                            new PolicyStatement().Allow().WithActions("events:PutEvents")
                                .WithResources(),                          
                        },
                    })}
                },
            });
            
            // Configure inbound security for RDP (and more?)
            var securityGroup = new SecurityGroup(this, $"Ingress", new SecurityGroupProps
            {
                Vpc = Vpc,
                AllowAllOutbound = true,
            });
            foreach(var rule in props.IngressRules)
            {
                securityGroup.WithIngressRule(rule);    
            }

            //var eip = new CfnEIP(this, "IvrEIP", new CfnEIPProps            {            });            WriteLine($"EIP: {eip.LogicalId}");
             
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
                UserData = HostPriming.PrimeForS3i(hostProps).UserData,
            };
            this.Instance = new Instance_(this, $"CallHost", instanceProps);
        }
    }
}
