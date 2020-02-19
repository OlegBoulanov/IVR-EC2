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
        public string WorkingFolder { get; set; }
        public string LogFilePath { get; protected set; }
        public WindowsCommands WithWorkingFolder(string workingFolder)
        {
            WorkingFolder = workingFolder;
            return this;
        }
        public WindowsCommands WithLogFile(string logFilePath)
        {
            LogFilePath = logFilePath;
            return this;
        }
        public WindowsCommands Log(string s)
        {
            if(!string.IsNullOrWhiteSpace(LogFilePath))
            {
                UserData.AddCommands($"Add-Content -Path \"{LogFilePath}\" -Force -Value \"$(Get-Date -Format \"HH:mm:ss.fff\"): {s.Replace("\n", "`n").Replace("\"", "`\"")}\"");
            }
            return this;
        }
        public WindowsCommands WithCommands(string commands)
        {
            Console.WriteLine($"PS> {commands.Replace("`n", " ; ")}");
            Log($"PS> {commands}");
            UserData.AddCommands($"{commands}");
            return this;
        }
        public WindowsCommands WithDownload(string path, string outfile = null)
        {
            if(string.IsNullOrWhiteSpace(outfile)) outfile = Path.GetFileName(path);
            return WithCommands($"wget \"{path}\" -Outfile \"{outfile}\"");
        }        
        public WindowsCommands WithInstall(string localFile, string installArgs)
        {
            switch (Path.GetExtension(localFile).ToLower(CultureInfo.CurrentCulture))
            {
                case ".exe":
                    WithCommands($"./{localFile} {installArgs} | Out-Null");
                    break;
                case ".msi":
                    WithCommands($"msiexec /i \"{localFile}\" {installArgs} | Out-Null");
                    break;
            }
            return this;
        }
        public WindowsCommands WithDownloadAndInstall(params string[] products)
        {
            foreach (var pathAndArgs in products)
            {
                var ps = pathAndArgs.IndexOfAny(new char[] { ' ', '\t', ',', ';', });
                var remotePath = 0 < ps ? pathAndArgs.Substring(0, ps) : pathAndArgs;
                var installArgs = 0 < ps ? pathAndArgs.Substring(ps) : "";
                var localFile = Path.GetFileName(remotePath);
                WithDownload(remotePath, localFile).WithInstall(localFile, installArgs);
            }
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
            var relativePath = ".aws\\credentials";
            var profileData = $"[default]\ncredential_source = Ec2InstanceMetadata\nrole_arn = arn:aws:iam::{account}:role/{role}";
            if(string.IsNullOrWhiteSpace(user)) {
                // set for both 64 and 32 bit apps
                return WithFile($"C:\\Windows\\System32\\config\\systemprofile\\{relativePath}", profileData)
                      .WithFile($"C:\\Windows\\SysWOW64\\config\\systemprofile\\{relativePath}", profileData);
            } 
            return WithFile($"C:\\Users\\{user}\\{relativePath}", profileData);
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
        public WindowsCommands WithCredentials(string username, string password, string credentials = "$credentials")
        {
            return WithCommands($"${credentials} = New-Object System.Management.Automation.PSCredential -ArgumentList @(\"{username}\",(ConvertTo-SecureString -String \"{password}\" -AsPlainText -Force))");
        }
        public WindowsCommands WithExplorerSettingsFile(string path, int hidden = 1, int hideFileExt = 0)
        {
            var settings = "Windows Registry Editor Version 5.00\n";
            settings += $"[HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced]\n";
            settings += $"\"Hidden\"=DWORD:{hidden}\n";
            settings += $"\"HideFileExt\"=DWORD:{hideFileExt}\n";
            return WithFile(path, settings);//.WithCommands($"regedit /s {path}");
        }
        public WindowsCommands WithNewFolder(string newFolderPath, bool setLocation = false)
        {
            WithCommands($"New-Item -ItemType Directory -Force -Path \"{newFolderPath}\"");
            if(setLocation) {
                WithCommands($"Set-Location \"{newFolderPath}\"");
                WorkingFolder = newFolderPath;
            }
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