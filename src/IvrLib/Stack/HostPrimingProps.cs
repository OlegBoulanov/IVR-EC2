using System;
using System.Collections.Generic;
using System.Linq;

namespace IvrLib
{
    public class HostPrimingProps
    {
        public string HostName { get; set; }
        public string WorkingFolder { get; set; } = "CDK_Stack";
        public string AwsAccount { get; set; }
        public string AwsRoleName { get; set; }
        public RdpProps RdpProps { get; set; }
        public IEnumerable<string> DownloadAndInstall { get; set; }
        public IEnumerable<string> EC2Users { get; set; }
        public string S3iArgs { get; set; }
        public static string S3iRelease { get; set; } = "https://github.com/OlegBoulanov/s3i/releases/download/v1.0.328/s3i.msi";
    }
}