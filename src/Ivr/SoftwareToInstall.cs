using System;
using System.IO;
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
    public class SoftwareToInstall
    {

        // install software            
        public static string vc_2015 = "https://aka.ms/vs/16/release/vc_redist.x86.exe";
        public static string dotnet30 = "https://download.visualstudio.microsoft.com/download/pr/d12cc6fa-8717-4424-9cbf-d67ae2fb2575/b4fff475e67917918aa2814d6f673685/dotnet-runtime-3.0.1-win-x64.exe";

        // https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script
        public static string dotNetInstallScript = "https://dot.net/v1/dotnet-install.ps1";
        public static string s3i = "https://github.com/OlegBoulanov/s3i/releases/download/v1.0.315/s3i.msi";

        public static string s3i_config = "https://s3-ap-southeast-2.amazonaws.com/install.ap-southeast-2.elizacorp.com/CHAU-01/s3i.ini";
    }
}