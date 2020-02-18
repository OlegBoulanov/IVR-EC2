using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;

namespace IvrLib
{
    public class IvrStackProps : StackProps
    {
        //public string KeyName { get; set; }
        public string RdpUserName { get; set; }
        public string RdpUserPassword { get; set; }
        public IEnumerable<string> UserGroups { get; set; } = new List<string> { "Administrators", "Remote Desktop Users" };
        public IEnumerable<IngressRule> IngressRules { get; set; }
        public string BucketsDomain { get; set; }   // .useast1.company.com, region related, but doesn't have to match region exactly
        public IEnumerable<string> EC2Users { get; set; }
        public string s3i_args { get; set; }    // = "https://raw.githubusercontent.com/OlegBoulanov/s3i/develop/Examples/Config.ini --verbose";

        public string [] S3BucketResources(params string [] prefixes)
        {
            return prefixes.Select(p => $"arn:aws:s3:::{p}.{BucketsDomain}").ToArray();
        }
        public string [] S3ObjectResources(params string [] prefixes)
        {
            return prefixes.Select(p => $"arn:aws:s3:::{p}.{BucketsDomain}/*").ToArray();
        }
    }
}