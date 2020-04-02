using System;
using System.Collections.Generic;
using System.Linq;

namespace IvrLib
{
    public class HostGroup
    {
        public IvrInstanceProps InstanceProps { get; set; } = new IvrInstanceProps();
        public IEnumerable<string> DownloadAndInstall { get; set; }
        public string InstallS3i { get; set; }
        public bool AllocateElasticIPs { get; set; } = false;
        public int HostCount { get; set; } = 1;
    }
}