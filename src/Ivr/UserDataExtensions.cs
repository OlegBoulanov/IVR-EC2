using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace Ivr
{
    public static class UserDataExtensions
    {
        public static Amazon.CDK.AWS.EC2.UserData WithCommands(this Amazon.CDK.AWS.EC2.UserData data, string commands)
        {
            data.AddCommands(commands);
            return data;
        }
        public static Amazon.CDK.AWS.EC2.UserData WithDownload(this Amazon.CDK.AWS.EC2.UserData data, string path, string outfile = null)
        {
            if(string.IsNullOrWhiteSpace(outfile)) outfile = Path.GetFileName(path);
            data.AddCommands($"wget {path} -outfile {outfile}");
            return data;
        }        
        public static Amazon.CDK.AWS.EC2.UserData WithInstall(this Amazon.CDK.AWS.EC2.UserData data, string path)
        {
            var fileName = Path.GetFileName(path);
            switch (Path.GetExtension(fileName).ToLower(CultureInfo.CurrentCulture))
            {
                case ".exe":
                    data.AddCommands($"./{fileName} /s | Out-Null");
                    break;
                case ".msi":
                    data.AddCommands($"msiexec /i {fileName} /quiet | Out-Null");
                    break;
            }
            return data;
        }
        public static Amazon.CDK.AWS.EC2.UserData WithDownloadAndInstall(this Amazon.CDK.AWS.EC2.UserData data, params string[] paths)
        {
            foreach(var path in paths) data.WithDownload(path, null).WithInstall(path);
            return data;
        }
        public static Amazon.CDK.AWS.EC2.UserData WithEnvironmentVariables(this Amazon.CDK.AWS.EC2.UserData data, params IDictionary<string, string>[] vars)
        {
            foreach(var var in vars) foreach(var kv in var) data.WithCommands($"setx /m {kv.Key} {kv.Value}");
            return data;
        }
        public static Amazon.CDK.AWS.EC2.UserData WithFiles(this Amazon.CDK.AWS.EC2.UserData data, params IDictionary<string, string>[] allfiles)
        {
            foreach(var files in allfiles) foreach(var file in files)
            {
                data.WithCommands($"new-item -force {file.Key}");
                data.WithCommands($"add-content {file.Key} -value \"{file.Value.Replace("\n", "`n")}\"");
            }
            return data;
        }
        public static Amazon.CDK.AWS.EC2.UserData WithEc2Credentials(this Amazon.CDK.AWS.EC2.UserData data, string user, string account, string role)
        {
            return data.WithFiles(new Dictionary<string, string>{
                { $"C:\\Users\\{user}\\.aws/credentials", $"[default]\ncredential_source = Ec2InstanceMetadata\nrole_arn = arn:aws:iam::{account}:role/{role}" },
            });
        }
        public static Amazon.CDK.AWS.EC2.UserData WithNewUser(this Amazon.CDK.AWS.EC2.UserData data, string userName, string userPassword, params string [] addToGroups)
        {
            data.AddCommands($"$Pwd = ConvertTo-SecureString \"{userPassword}\" -AsPlainText -Force");
            data.AddCommands($"New-LocalUser -Name \"{userName}\" -Password $Pwd -Description \"User created by CDK Deploy Command\"");
            foreach(var group in addToGroups) 
            {
                data.AddCommands($"Add-LocalGroupMember -group \"{group}\" -Member \"{userName}\"");
            }
            return data;
        }
        public static Amazon.CDK.AWS.EC2.UserData WithNewFolder(this Amazon.CDK.AWS.EC2.UserData data, string newFolderPath)
        {
            data.AddCommands($"$NewFolderPath=\"{newFolderPath}\"");
            data.AddCommands($"New-Item -ItemType Directory -Force -Path \"$NewFolderPath\"");
            data.AddCommands($"Set-Location \"$NewFolderPath\"");
            return data;
        }
    }
}