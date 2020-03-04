using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.RegionInfo;

using IvrLib.Security;
using IvrLib.Utils;

namespace IvrLib
{
    public class IvrStackProps : StackProps
    {
        public RegionInfo RegionInfo { get; set; }
        public string KeyPairName { get; set; }
        public string RdpUserName { get; set; }
        public string RdpUserPassword { get; set; }
        public IEnumerable<string> RdpUserGroups { get; set; } = new List<string> { "Administrators", "Remote Desktop Users" };
        public IEnumerable<SecurityGroupRule> SecurityGroupRules { get; set; }
        public bool AllowAllOutbound { get; set; } = true;
        public string BucketsDomain { get; set; }   // .useast1.company.com, region related, but doesn't have to match region exactly
        public string HostsDomainName {get; set; }   // host32.company.domain.net
        public IEnumerable<string> EC2Users { get; set; }
        public string s3i_args { get; set; }    // = "https://raw.githubusercontent.com/OlegBoulanov/s3i/develop/Examples/Config.ini --verbose";
        public string [] S3BucketResources(params string [] prefixes)
        {
            return prefixes.Select(prefix => $"arn:aws:s3:::{prefix}.{BucketsDomain}").ToArray();
        }
        public string [] S3ObjectResources(params string [] prefixes)
        {
            return S3BucketResources(prefixes).Select(bucket => $"{bucket}/*").ToArray();
        }
    }
}