using System;
using System.Collections.Generic;
using System.Linq;

using YamlDotNet.Serialization;

using IvrLib.Utils;

namespace IvrLib
{
    public class IvrSiteSchema
    {
        public string SiteName { get; set; }
        public IDictionary<string, string> Define { get; set; } = new Dictionary<string, string> {};
        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string> {};
        public IvrVpcProps VpcProps { get; set; }   
        public string VpcName { get; set; }
        public string VpcId { get; set; }   
        public int MaxSubnets { get; set; } = 10;
        public string KeyPairName { get; set; }
        public RdpProps RdpProps { get; set; }
        public IEnumerable<string> EC2Users { get; set; }
        // Pre-registered site domain name
        public string HostedZoneDomain { get; set; }
        public string SubdomainEIPs { get; set; }
        public string SubdomainHosts { get; set; }
        public string HostNamePrefix { get; set; } = "CH-";
        public IEnumerable<string> PreAllocatedElasticIPs { get; set; }
        public IEnumerable<HostGroup> HostGroups { get; set; }
        public IEnumerable<string> SipProviders { get; set; } = new List<string> { "Amazon", "Twilio", };
        public IEnumerable<PortSpec> IngressPorts { get; set; }
        public bool AllowAllOutbound { get; set; } = true;
        public bool AllowAllIntranet { get; set; } = true;
        public bool AddVpcS3Gateway { get; set; } = true;
        public S3Buckets S3Buckets { get; set; }
        public string S3iRelease { get; set; }
        public string ResolveProperty(Context ctx, string prop, string propName, string cvarName = null)
        {
            if (string.IsNullOrWhiteSpace(prop)) {
                if(string.IsNullOrWhiteSpace(cvarName)) cvarName = propName;
                prop = ctx.Resolve(cvarName, help: $"Please define {nameof(IvrSiteSchema)}.{propName}, or use -c {cvarName}=<value>");
            }
            return prop;
        }
        public IvrSiteSchema ResolveUndefinedProperties(Context ctx)
        {
            if (string.IsNullOrWhiteSpace(KeyPairName))
            {
                RdpProps.UserName = ResolveProperty(ctx, RdpProps.UserName, $"{nameof(RdpProps)}.{nameof(RdpProps.UserName)}", $"Rdp.User");
                RdpProps.Password = ResolveProperty(ctx, RdpProps.Password, $"{nameof(RdpProps)}.{nameof(RdpProps.Password)}", $"Rdp.Password");
            }
            if(0 == (RdpProps.Cidrs?.Count() ?? 0))
            {
                RdpProps.Cidrs = ResolveProperty(ctx, null, $"{nameof(RdpProps)}.{nameof(RdpProps.Cidrs)}", $"Rdp.Cidrs").Csv();
            }
            return this;
        }
        public IvrSiteSchema Validate()
        {
            if(0 == SipProviders.Count()) 
                throw new ArgumentException($"No SIP Providers defined in schema");
            if(null == RdpProps.UserGroups || !RdpProps.UserGroups.Contains("RdpUsers")) 
                throw new ArgumentException($"{nameof(RdpProps.UserGroups)} must include 'RdpUsers'");
            if(null == RdpProps.Cidrs || 0 == RdpProps.Cidrs.Count())
                throw new ArgumentException($"No {nameof(RdpProps.Cidrs)} defined");
            return this;
        }
        public IvrSiteSchema Preprocess()
        {
            // Add RdpUser and/or Administrator to EC2Users
            if(null == EC2Users || 0 == EC2Users.Count()) EC2Users = new List<string> { RdpProps.UserName ?? "Administrator" };
            // override defaults if values provided
            if(!string.IsNullOrWhiteSpace(S3iRelease)) HostPrimingProps.S3iRelease = S3iRelease;
            // unwrap CSVs
            SipProviders = SipProviders.SelectMany(n => n.Csv()).ToList();
            return this;
        }
        public string [] S3Resources(string suffix, params string [] prefixes)
        {
            var dotSuffix = string.IsNullOrWhiteSpace(S3Buckets.Suffix) ? "" : $".{S3Buckets.Suffix}";
            return prefixes.Select(prefix => $"arn:aws:s3:::{prefix}{dotSuffix}{suffix}").ToArray();
        }
        public static IvrSiteSchema FromString(string s)
        {
            var schemaText = System.Environment.ExpandEnvironmentVariables(s);
            var ds = new DeserializerBuilder().Build();
            var schema = ds.Deserialize<IvrSiteSchema>(schemaText);
            var defines = schema.Define;
            schema.Define = null;   // drop defines for now
            schemaText = new SerializerBuilder().Build().Serialize(schema);
            // and expand defines in schema text
            foreach (var d in defines) schemaText = schemaText.Replace(d.Key, d.Value);
            // deserialize with all defines expanded
            schema = ds.Deserialize<IvrSiteSchema>(schemaText);
            schema.Define = defines;
            return schema;
        }
    }
}