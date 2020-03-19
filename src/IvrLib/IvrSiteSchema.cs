using System;
using System.Collections.Generic;
using System.Linq;

using IvrLib.Utils;

namespace IvrLib
{
    public class IvrSiteSchema
    {
        public IvrVpcProps VpcProps { get; set; } = new IvrVpcProps {};       
        public string KeyPairName { get; set; }
        public RdpProps RdpProps { get; set; }
        public IEnumerable<string> EC2Users { get; set; }
        // Pre-registered site domain name
        public string Domain { get; set; }
        public IEnumerable<HostGroup> HostGroups { get; set; }
        public IEnumerable<string> SipProviders { get; set; } = new List<string> { "Amazon", "Twilio", };
        public IEnumerable<PortSpec> IngressPorts { get; set; }
        public bool AllowAllOutbound { get; set; } = true;
        public string ResourceSuffix { get; set; } = "something.unique"/*.<region>.amazonaws.com*/;
        public bool Validate()
        {
            if(string.IsNullOrWhiteSpace(KeyPairName) && (string.IsNullOrWhiteSpace(RdpProps.UserName) || string.IsNullOrWhiteSpace(RdpProps.Password))) 
                    throw new ArgumentException($"{nameof(KeyPairName)} or {nameof(RdpProps.UserName)} must not be provided");
            if(0 == SipProviders.Count()) 
                throw new ArgumentException($"No SIP Providers defined in schema");
            if(null == RdpProps.UserGroups || !RdpProps.UserGroups.Contains("RdpUsers")) 
                throw new ArgumentException($"{nameof(RdpProps.UserGroups)} must include 'RdpUsers'");
                if(null == RdpProps.Cidrs || 0 == RdpProps.Cidrs.Count())
                    throw new ArgumentException($"No {nameof(RdpProps.Cidrs)} defined");
            return true;
        }
        public IvrSiteSchema Preprocess()
        {
            // Add RdpUser and/or Administrator to EC2Users
            if(null == EC2Users || 0 == EC2Users.Count()) EC2Users = new List<string> { RdpProps.UserName ?? "Administrator" };
            return this;
        }
        public string [] S3BucketResources(params string [] prefixes)
        {
            return prefixes.Select(prefix => $"arn:aws:s3:::{prefix}.{ResourceSuffix}.amazonaws.com").ToArray();
        }
        public string [] S3ObjectResources(params string [] prefixes)
        {
            return S3BucketResources(prefixes).Select(bucket => $"{bucket}/*").ToArray();
        }
    }
}