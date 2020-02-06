using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace Ivr
{
    public class UserData
    {

        // install software            
        public static string dotnet30 = "https://download.visualstudio.microsoft.com/download/pr/d12cc6fa-8717-4424-9cbf-d67ae2fb2575/b4fff475e67917918aa2814d6f673685/dotnet-runtime-3.0.1-win-x64.exe";
        public static string s3i = "https://github.com/OlegBoulanov/s3i/releases/download/v1.0.315/s3i.msi";

        public static Amazon.CDK.AWS.EC2.UserData Create()
        {
            var data = Amazon.CDK.AWS.EC2.UserData.ForWindows();
            data.AddCommands($"$path=\"C:\\ProgramData\\EC2\\\"");
            data.AddCommands($"New-Item -ItemType Directory -Force -Path \"$path\"");
            data.AddCommands($"Set-Location \"$path\"");

            data.AddCommands($"wget {dotnet30} -outfile dotnet-runtime.exe");
            data.AddCommands($"./dotnet-runtime.exe /s");

            data.AddCommands($"wget {s3i} -outfile s3i.msi");
            data.AddCommands($"msiexec -i s3i.msi -qn");
            //data.AddCommands(""); 

            return data;
        }
    }
}
