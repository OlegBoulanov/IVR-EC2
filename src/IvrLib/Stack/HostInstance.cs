using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class HostInstance
    {
        public Instance_ Instance { get; set; }
        public HostGroup Group { get; set; }
        public string ElasticIPAllocationId { get; set; }
    }
}