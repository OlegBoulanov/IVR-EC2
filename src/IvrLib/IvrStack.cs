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

            //var publicSubnets = Vpc.PublicSubnets;
            //WriteLine($"{publicSubnets.Aggregate($"{nameof(IvrStack)}.PublicSubnets[{publicSubnets.Length}]:", (a, subnet) => { return $"{a}{System.Environment.NewLine}  {subnet.SubnetId}/{subnet.AvailabilityZone} => {subnet.RouteTable.RouteTableId}"; })}");

            var amiImage = new WindowsImage(WindowsVersion.WINDOWS_SERVER_2019_ENGLISH_FULL_BASE);
            var amiImageConfig = amiImage.GetImage(this);
            //WriteLine($"Win: {amiImageConfig.OsType}/{amiImageConfig.ImageId}");

            var role = new Role(this, "Role_CallHost", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
                InlinePolicies = new Dictionary<string, PolicyDocument> {
                    { "IvrPolicy", new PolicyDocument(new PolicyDocumentProps {
                        Statements = new PolicyStatement[] {
                            new PolicyStatement(new PolicyStatementProps{
                                Effect = Effect.ALLOW,
                                Actions = new string [] { "sts:AssumeRole" },
                                Resources = new string [] { $"arn:aws:iam::{props.Env.Account}:role/IvrStack*" },
                            }),
                        },
                    })}
                },
            });
            
            var securityGroup = new SecurityGroup(this, $"InboundRDP", new SecurityGroupProps
            {
                Vpc = Vpc,
                AllowAllOutbound = true,
            });
            // add IB RDP 
            foreach(var rule in props.IngressRules)
            {
                securityGroup.AddIngressRule(Peer.Ipv4(rule.Key), Port.Tcp(rule.Value), $"Ingress: {rule.Key}:{rule.Value}");    
            }

            //var eip = new CfnEIP(this, "IvrEIP", new CfnEIPProps            {            });            WriteLine($"EIP: {eip.LogicalId}");

            var workingFolder = $"C:\\ProgramData\\{id}";
            var commandsToRun = new WindowsCommands()
                .WithWorkingFolder(workingFolder);

            commandsToRun
                // working folder and log file
                .WithNewFolder(workingFolder)
                .WithLogFile($"{workingFolder}\\{id}.log").Log($"User profile: $env:userprofile")    // C:\Users\Administrator
                // RDP user
                .WithNewUser(props.UserName, props.UserPassword, props.UserGroups.ToArray());

            commandsToRun
                // more AWS-enabled users
                .WithEc2Credentials(props.UserName, props.Env.Account, role.RoleName)
                .WithEc2Credentials(null, props.Env.Account, role.RoleName); // system

            props.EC2Users?.ToList().ForEach(user => {
                //WriteLine($"EC2User: {user}");
                commandsToRun.WithEc2Credentials(user, props.Env.Account, role.RoleName);
            });

            if(!string.IsNullOrWhiteSpace(props.s3i_args)) {
                //WriteLine($"s3i_args: {props.s3i_args}");
                commandsToRun.WithEnvironmentVariable("s3i_args", $"{props.s3i_args} --stage {workingFolder}\\s3i\\");
            }

            commandsToRun
                .WithDownloadAndInstall("https://aka.ms/vs/16/release/vc_redist.x86.exe /s"
                    , "https://download.visualstudio.microsoft.com/download/pr/9f010da2-d510-4271-8dcc-ad92b8b9b767/d2dd394046c20e0563ce5c45c356653f/dotnet-runtime-3.1.0-win-x64.exe /s"
                    , "https://awscli.amazonaws.com/AWSCLIV2.msi /quiet"
                    //, "https://github.com/OlegBoulanov/s3i/releases/download/v1.0.322/s3i.msi /quiet"
                );
                
                /*
            commandsToRun
                .WithDisableUAC(restartComputer: false)
                // more before restarting?
                .WithRestart();
                
                /*
                ...reboot to complete fixing UAC, and s3i will kick in at restart...
                */

            this.Instance = new Instance_(this, $"CallHost", new InstanceProps
            {
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.LARGE),
                
                MachineImage = amiImage,
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
                
                //KeyName = props.KeyName,
                Role = role,
                SecurityGroup = securityGroup,
                VpcSubnets = new SubnetSelection
                {
                    SubnetType = SubnetType.PUBLIC
                },
                UserData = commandsToRun.UserData,
            });
        }
    }
}
