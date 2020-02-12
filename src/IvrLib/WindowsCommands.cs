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
            if(!string.IsNullOrWhiteSpace(LogFilePath)) WithCommands($"new-item -force {LogFilePath}");
            return this;
        }
        public WindowsCommands Log(string s)
        {
            if(!string.IsNullOrWhiteSpace(LogFilePath))
            {
                //WriteLine($"{DateTime.Now:HH:mm:ss.fff}: {s}");
                UserData.AddCommands($"Add-Content {LogFilePath} -value \"$(Get-Date -Format \"HH:mm:ss.fff\"): {s.Replace("\n", "`n")}\"");
            }
            return this;
        }
        public WindowsCommands WithCommands(string commands, bool logOutput = true)
        {
            Log($"Command(s): {commands}");
            //UserData.AddCommands($"{commands}{(logOutput && string.IsNullOrWhiteSpace(LogFilePath) ? "" : " | Add-Content {LogFilePath}")}");
            UserData.AddCommands($"{commands}");
            return this;
        }
        public WindowsCommands WithDownload(string path, string outfile = null)
        {
            if(string.IsNullOrWhiteSpace(outfile)) outfile = Path.GetFileName(path);
            return WithCommands($"WGet {path} -Outfile {outfile}", logOutput: false);
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
            return WithCommands($"Setx /m {name} \"{value}\" | Out-Null");
        }
        public WindowsCommands WithFiles(params IDictionary<string, string>[] allfiles)
        {
            foreach(var files in allfiles) foreach(var file in files)
            {
                WithCommands($"New-Item -Force {file.Key}");
                WithCommands($"Add-Content {file.Key} -Value \"{file.Value.Replace("\n", "`n")}\"");
            }
            return this;
        }
        public WindowsCommands WithEc2Credentials(string user, string account, string role)
        {
            // https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-metahtml
            return WithFiles(new Dictionary<string, string>{
                { $"C:\\Users\\{user}\\.aws/credentials", $"[default]\ncredential_source = Ec2InstanceMetadata\nrole_arn = arn:aws:iam::{account}:role/{role}" },
            });
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
            WithCommands($"$NewFolderPath=\"{newFolderPath}\"");
            WithCommands($"New-Item -ItemType Directory -Force -Path \"$NewFolderPath\"");
            if(setLocation) WithCommands($"Set-Location \"$NewFolderPath\"");
            return this;
        }
        public WindowsCommands WithDisableUAC(bool restartComputer = false)
        {
            // https://support.gfi.com/hc/en-us/articles/360012968753-Disabling-the-User-Account-Control-UAC-
            WithCommands($"New-ItemProperty -Path HKLM:Software\\Microsoft\\Windows\\CurrentVersion\\policies\\system -Name EnableLUA -PropertyType DWord -Value 0 -Force | Out-Null");
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