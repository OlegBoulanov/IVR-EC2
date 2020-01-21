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
        public Instance_ Instance {get; protected set; }
        public UserData UserData { get; protected set; }
        internal IvrStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {

            var availabilityZones = this.AvailabilityZones;
            WriteLine($"{availabilityZones.Aggregate($"{nameof(IvrStack)}.AZ[{availabilityZones.Length}]:", (a, zone) => { return $"{a}{System.Environment.NewLine}  {zone}"; })}");

            this.Vpc = Amazon.CDK.AWS.EC2.Vpc.FromLookup(this, "Vpc", new VpcLookupOptions
            {
                VpcName = "120"
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
            role.AddManagedPolicy(new IvrPolicy(this, "IvrPolicy"));
            WriteLine($"Role: {role.RoleArn}");

            //var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            var securityGroup = new SecurityGroup(this, "IvrSecurityGroup", new SecurityGroupProps
            {
                Vpc = Vpc,
                AllowAllOutbound = true,
            });
            securityGroup.AddIngressRule(Peer.Ipv4($"10.27.0.0/16"), Port.Tcp(3389), "RDP-Private");
            var myPublicIP = System.Environment .GetEnvironmentVariable("RDP_PUBLIC_IP");
            if(!string.IsNullOrWhiteSpace(myPublicIP)) {
                securityGroup.AddIngressRule(Peer.Ipv4($"{myPublicIP}/32"), Port.Tcp(3389), "RDP-Public");
            } 
            else {
                WriteLine($"??? CDK_PUBLIC_IP is not set, No RDP access will be added");
            }

            WriteLine($"SG: {securityGroup.SecurityGroupVpcId}");

            // install software
            var dotnet30 = "https://download.visualstudio.microsoft.com/download/pr/d12cc6fa-8717-4424-9cbf-d67ae2fb2575/b4fff475e67917918aa2814d6f673685/dotnet-runtime-3.0.1-win-x64.exe";
            var s3i = "https://github.com/OlegBoulanov/s3i/releases/download/v1.0.315/s3i.msi";

            this.UserData = UserData.ForWindows();
            this.UserData.AddCommands($"$path=\"C:\\ProgramData\\EC2\\\"");
            this.UserData.AddCommands($"New-Item -ItemType Directory -Force -Path \"$path\"");
            this.UserData.AddCommands($"Set-Location \"$path\"");

            this.UserData.AddCommands($"wget {dotnet30} -outfile dotnet-runtime.exe");
            this.UserData.AddCommands($"./dotnet-runtime.exe /s");

            this.UserData.AddCommands($"wget {s3i} -outfile s3i.msi");
            this.UserData.AddCommands($"msiexec -i s3i.msi -qn");
            //this.UserData.AddCommands("");


            var eip = new CfnEIP(this, "IvrEIP", new CfnEIPProps
            {
                
            });
            WriteLine($"EIP: {eip.LogicalId}");
 
            this.Instance = new Instance_(this, "IvrStackInstance", new InstanceProps
            {
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.XLARGE),
                MachineImage = amiImage,
                Vpc = Vpc,
                BlockDevices = new BlockDevice[]
                {
                    new BlockDevice
                    {
                        DeviceName = "/dev/sda1",
                        Volume = BlockDeviceVolume.Ebs(30, new EbsDeviceOptions
                        { 
                            VolumeType = EbsDeviceVolumeType.STANDARD}
                        )
                    },
                },
                KeyName = "IvrTestKey",
                Role = role,
                SecurityGroup = securityGroup,
                VpcSubnets = new SubnetSelection
                {
                    SubnetType = SubnetType.PUBLIC
                },
                UserData = UserData,
            });
        }
    }
}
