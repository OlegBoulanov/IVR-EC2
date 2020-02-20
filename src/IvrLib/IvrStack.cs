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

            // Now is time to assemble PowerShell command to execute at first start
            var workingFolder = $"C:\\ProgramData\\{id}";
            var explorerSettingsPath = $"{workingFolder}\\explorer_settings.reg";
            var commandsToRun = new WindowsCommands()
                // working folder and log file
                .WithNewFolder(workingFolder, setLocation: true)
                .WithLogFile($"{workingFolder}\\{id}.log").Log($"User profile: $env:userprofile")    // C:\Users\Administrator
                .WithExplorerSettingsFile(explorerSettingsPath, hidden: 1, hideFileExt: 0);

            // Create RDP user first
            if(!string.IsNullOrWhiteSpace(props.RdpUserName)) {
                commandsToRun 
                    .WithNewUser(props.RdpUserName, props.RdpUserPassword, props.UserGroups.ToArray())
                    .WithCredentials(props.RdpUserName, props.RdpUserPassword, "$creds")
                    .WithStartProcess("regedit", $"/s {explorerSettingsPath}", "$creds");
            }

            // AWS-enable certain users
            commandsToRun
                .WithEc2Credentials("$Env:USERNAME", props.Env.Account, role.RoleName)  // current user (Administrator)
                .WithEc2Credentials(null, props.Env.Account, role.RoleName);            // system, as s3i service account
            // ...and enable more
            props.EC2Users?.ToList().ForEach(user => {
                commandsToRun.WithEc2Credentials(user, props.Env.Account, role.RoleName);
            });

            // Download and install bare minimum: VC runtime, .NET Core, AWS CLI, ...
            commandsToRun
                .WithDownloadAndInstall($"https://aka.ms/vs/16/release/vc_redist.x86.exe /s",
                    $"https://download.visualstudio.microsoft.com/download/pr/9f010da2-d510-4271-8dcc-ad92b8b9b767/d2dd394046c20e0563ce5c45c356653f/dotnet-runtime-3.1.0-win-x64.exe /s",
                    $"https://awscli.amazonaws.com/AWSCLIV2.msi /quiet"
                );

            // If requested, install and run s3i to install the rest indirectly from specific remote configuration
            if(!string.IsNullOrWhiteSpace(props.s3i_args)) {
                commandsToRun
                    .WithDownloadAndInstall($"https://github.com/OlegBoulanov/s3i/releases/download/v1.0.328/s3i.msi /quiet")
                    .WithEnvironmentVariable("s3i_args", $" --stage {workingFolder}\\s3i {props.s3i_args}")
                    .WithCommands("Restart-Service -Name s3i -Force");  // install products frome the line above
            }

// $timeout=8; $timer=[Diagnostics.StopWatch]::StartNew();while(($timer.Elapsed.TotalSeconds -lt $timeout)) { Start-Sleep -Seconds 1; Write-Host $timer.Elapsed.TotalSeconds };$timer.Stop();

            // final touches and reboot
            commandsToRun
                .WithDisableUAC(restartComputer: false)
                .WithCommands($"Rename-Computer {id}")
                // anything else to do - before restarting?
                .WithRestart(); // ...reboot to complete fixing UAC/renaming...
                
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
                UserData = commandsToRun.UserData,
            };
            this.Instance = new Instance_(this, $"CallHost", instanceProps);
        }
    }
}
