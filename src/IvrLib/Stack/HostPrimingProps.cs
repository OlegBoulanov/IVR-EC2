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
        public IEnumerable<string> EC2Users { get; set; }
        public string S3iArgs { get; set; }
    }
}