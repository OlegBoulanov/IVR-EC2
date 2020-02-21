using System;
using System.Linq;

namespace IvrLib
{
    public class HostPriming
    {
        public static WindowsCommands PrimeForS3i(HostPrimingProps props)
        {
            var workingFolder = $"C:\\ProgramData\\{props.HostName}";
            var explorerSettingsPath = $"{workingFolder}\\explorer_settings.reg";
            var commandsToRun = new WindowsCommands()
                // working folder and log file
                .WithNewFolder(workingFolder, setLocation: true)
                .WithLogFile($"{workingFolder}\\{props.HostName}.log").Log($"User profile: $env:userprofile")    // C:\Users\Administrator
                .WithExplorerSettingsFile(explorerSettingsPath, hidden: 1, hideFileExt: 0)
                .Log();

            // Create RDP user first
            if(!string.IsNullOrWhiteSpace(props.RdpUserName)) {
                commandsToRun 
                    .WithNewUser(props.RdpUserName, props.RdpUserPassword, props.RdpUserGroups.ToArray())
                    .WithCredentials(props.RdpUserName, props.RdpUserPassword, "$creds")
                    .WithStartProcess("regedit", $"/s {explorerSettingsPath}", "$creds")
                    .Log();
            }

            // AWS-enable certain users
            commandsToRun
                .WithEc2Credentials("$Env:USERNAME", props.AwsAccount, props.AwsRoleName)  // current user (Administrator)
                .WithEc2Credentials(null, props.AwsAccount, props.AwsRoleName)            // system, as s3i service account
                .Log();
            // ...and enable more, if needed
            props.EC2Users.ToList().ForEach(user => {
                commandsToRun.WithEc2Credentials(user, props.AwsAccount, props.AwsRoleName)
                    .Log();
            });

            // Download and install bare minimum: VC runtime, .NET Core, AWS CLI, ...
            commandsToRun
                .WithDownloadAndInstall($"https://aka.ms/vs/16/release/vc_redist.x86.exe /s",
                    $"https://download.visualstudio.microsoft.com/download/pr/9f010da2-d510-4271-8dcc-ad92b8b9b767/d2dd394046c20e0563ce5c45c356653f/dotnet-runtime-3.1.0-win-x64.exe /s",
                    $"https://awscli.amazonaws.com/AWSCLIV2.msi /quiet"
                )
                .Log();

            // If requested, install and run s3i to install the rest indirectly from specific remote configuration
            if(!string.IsNullOrWhiteSpace(props.S3iArgs)) {
                commandsToRun
                    .WithDownloadAndInstall($"https://github.com/OlegBoulanov/s3i/releases/download/v1.0.328/s3i.msi /quiet")
                    .WithEnvironmentVariable("s3i_args", $" --stage {workingFolder}\\s3i {props.S3iArgs}")
                    .WithCommands("Restart-Service -Name s3i -Force")  // install products frome the line above
                    .Log();
            }

// $timeout=8; $timer=[Diagnostics.StopWatch]::StartNew();while(($timer.Elapsed.TotalSeconds -lt $timeout)) { Start-Sleep -Seconds 1; Write-Host $timer.Elapsed.TotalSeconds };$timer.Stop();

            // final touches and reboot
            commandsToRun
                .WithDisableUAC(restartComputer: false)
                .WithCommands($"Rename-Computer {props.HostName}")
                // anything else to do - before restarting?
                .WithRestart(); // ...reboot to complete fixing UAC/renaming...

            return commandsToRun;
        }
    }
}