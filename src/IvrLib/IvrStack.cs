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

//            var eip = new CfnEIP(this, "IvrEIP", new CfnEIPProps
//            {
//            });
//            WriteLine($"EIP: {eip.LogicalId}");

            var workFolder = $"C:\\ProgramData\\{id}\\";
            var commandsToRun = new WindowsCommands();
            commandsToRun
                .WithNewUser(props.UserName, props.UserPassword, props.UserGroups)
                .WithNewFolder(workFolder)
                .WithLogFile($"{workFolder}\\{id}.log")
                .Log($"User profile: $env:userprofile")    // C:\Users\Administrator
                .WithDownloadAndInstall(
                    "https://aka.ms/vs/16/release/vc_redist.x86.exe",
                    "https://download.visualstudio.microsoft.com/download/pr/d12cc6fa-8717-4424-9cbf-d67ae2fb2575/b4fff475e67917918aa2814d6f673685/dotnet-runtime-3.0.1-win-x64.exe",
                    "https://awscli.amazonaws.com/AWSCLIV2.msi",
                    "https://github.com/OlegBoulanov/s3i/releases/download/v1.0.315/s3i.msi"
                )
                .WithEnvironmentVariable("s3i_args", $"\"https://raw.githubusercontent.com/OlegBoulanov/s3i/develop/Examples/Config.ini --stage {workFolder} --verbose\"")
                .WithEc2Credentials(props.UserName, props.Env.Account, role.RoleName)
                .WithEc2Credentials("OtherUser", props.Env.Account, role.RoleName)
                .WithDisableUAC(restartComputer: false)
                // more before restarting?
                .WithRestart();
                /*
                ...reboot to complete fixing UAC, and s3i will kick in at restart...
                */

            this.Instance = new Instance_(this, $"CallHost", new InstanceProps
            {
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.SMALL),
                
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
