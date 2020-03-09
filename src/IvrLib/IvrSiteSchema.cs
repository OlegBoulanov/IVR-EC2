using System;
using System.Collections.Generic;
using System.Linq;

using IvrLib.Utils;

namespace IvrLib
{
    public class IvrSiteSchema
    {
        public IvrVpcProps VpcProps { get; set; } = new IvrVpcProps {};       
        public string KeypairName { get; set; }
        public RdpProps RdpProps { get; set; }
        public IEnumerable<string> EC2Users { get; set; }
        public string Domain { get; set; }
        public string InstallFrom { get; set; }
        public int MaxAzs { get; set; } = 2;
        public IEnumerable<HostGroup> HostGroups { get; set; }
        public IEnumerable<string> SipProviders { get; set; } = new List<string> { "Amazon", "Twilio", };
        public IEnumerable<PortSpec> IngressPorts { get; set; }
        public bool AllowAllOutbound { get; set; } = true;
        public string S3DomainPrefix { get; set; } = "something.unique"/*.<region>.amazonaws.com*/;
        public bool Validate()
        {
            if(string.IsNullOrWhiteSpace(KeypairName) 
                && (string.IsNullOrWhiteSpace(RdpProps.UserName) || string.IsNullOrWhiteSpace(RdpProps.Password))) 
                    throw new ArgumentException($"{nameof(KeypairName)} or {nameof(RdpProps.UserName)} must not be provided");
            return true;
        }
        public IvrSiteSchema Preprocess()
        {
            // Add RdpUser and/or Administrator to EC2Users
            return this;
        }
        public string [] S3BucketResources(params string [] prefixes)
        {
            return prefixes.Select(prefix => $"arn:aws:s3:::{prefix}.{S3DomainPrefix}.amazonaws.com").ToArray();
        }
        public string [] S3ObjectResources(params string [] prefixes)
        {
            return S3BucketResources(prefixes).Select(bucket => $"{bucket}/*").ToArray();
        }
    }
}