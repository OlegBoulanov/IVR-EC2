using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace IvrLib
{
    public class WindowsCommands
    {
        public UserData UserData { get; protected set; } = UserData.ForWindows();
        public string LogFilePath { get; protected set; }
        public WindowsCommands WithLogFile(string logFilePath)
        {
            LogFilePath = logFilePath;
            return this;
        }
        public WindowsCommands Log(string s)
        {
            if(!string.IsNullOrWhiteSpace(LogFilePath))
            {
                //WriteLine($"{DateTime.Now:HH:mm:ss.fff}: {s}");
                UserData.AddCommands($"Add-Content -Path \"{LogFilePath}\" -Force -Value \"$(Get-Date -Format \"HH:mm:ss.fff\"): {s.Replace("\n", "`n").Replace("\"", "`\"")}\"");
            }
            return this;
        }
        public WindowsCommands WithCommands(string commands)
        {
            Log($"Command(s): {commands}");
            UserData.AddCommands($"{commands}");
            return this;
        }
        public WindowsCommands WithDownload(string path, string outfile = null)
        {
            if(string.IsNullOrWhiteSpace(outfile)) outfile = Path.GetFileName(path);
            return WithCommands($"wget \"{path}\" -Outfile \"{outfile}\"");
        }        
        public WindowsCommands WithInstall(string path)
        {
            var fileName = Path.GetFileName(path);
            switch (Path.GetExtension(fileName).ToLower(CultureInfo.CurrentCulture))
            {
                case ".exe":
                    WithCommands($"./{fileName} /s | Out-Null");
                    break;
                case ".msi":
                    WithCommands($"msiexec /i {fileName} /quiet | Out-Null");
                    break;
            }
            return this;
        }
        public WindowsCommands WithDownloadAndInstall(params string[] paths)
        {
            foreach(var path in paths) WithDownload(path, null).WithInstall(path);
            return this;
        }
        public WindowsCommands WithEnvironmentVariable(string name, string value)
        {
            return WithCommands($"Setx /m {name} \"{value}\"");
        }
        public WindowsCommands WithFile(string name, string content)
        {
            WithCommands($"New-Item -Force -Path \"{name}\"");
            return WithCommands($"Add-Content -Path \"{name}\" -Force -Value \"{content.Replace("\n", "`n")}\"");
        }        
        public WindowsCommands WithFiles(params IDictionary<string, string>[] allfiles)
        {
            foreach(var files in allfiles) foreach(var file in files)
            {
                WithCommands($"New-Item -Force -Path \"{file.Key}\"");
                WithCommands($"Add-Content -Path \"{file.Key}\" -Value \"{file.Value.Replace("\n", "`n")}\"");
            }
            return this;
        }
        public WindowsCommands WithEc2Credentials(string user, string account, string role)
        {
            // https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-metahtml
            var userprofile = string.IsNullOrWhiteSpace(user) ? "C:\\Windows\\System32\\config\\systemprofile" : $"C:\\Users\\{user}";
            Log($"Set EC2 creds for {user}, {account}/{role} => {userprofile}");
            return WithFile($"{userprofile}\\.aws\\credentials", $"[default]\ncredential_source = Ec2InstanceMetadata\nrole_arn = arn:aws:iam::{account}:role/{role}");
        }
        public WindowsCommands WithNewUser(string userName, string userPassword, params string [] addToGroups)
        {
            WithCommands($"$Pwd = ConvertTo-SecureString \"{userPassword}\" -AsPlainText -Force");
            WithCommands($"New-LocalUser -Name \"{userName}\" -Password $Pwd -Description \"User created by CDK Deploy Command\"");
            foreach(var group in addToGroups) 
            {
                WithCommands($"Add-LocalGroupMember -group \"{group}\" -Member \"{userName}\"");
            }
            return this;
        }
        public WindowsCommands WithNewFolder(string newFolderPath, bool setLocation = true)
        {
            WithCommands($"New-Item -ItemType Directory -Force -Path \"{newFolderPath}\"");
            if(setLocation) WithCommands($"Set-Location \"{newFolderPath}\"");
            return this;
        }
        public WindowsCommands WithDisableUAC(bool restartComputer = false)
        {
            // https://support.gfi.com/hc/en-us/articles/360012968753-Disabling-the-User-Account-Control-UAC-
            WithCommands($"New-ItemProperty -Path HKLM:Software\\Microsoft\\Windows\\CurrentVersion\\policies\\system -Name EnableLUA -PropertyType DWord -Value 0 -Force");
            if(restartComputer) { 
                WithRestart(); 
                return null;    // break the chain, triggering runtime error if misused
            }
            return this;
        }        
        public void WithRestart()
        {
            WithCommands("Restart-Computer");
            // no return, so compiler would complain if misused
        }
    }
}