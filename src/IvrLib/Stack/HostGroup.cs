using System;
using System.Collections.Generic;
using System.Linq;

namespace IvrLib
{
    public class HostGroup
    {
        public string Name { get; set; } = "Unnamed group";
        public IvrInstanceProps InstanceProps { get; set; } = new IvrInstanceProps();
        public IEnumerable<string> DownloadAndInstall { get; set; }
        public string InstallS3i { get; set; }
        public bool AllocateNewElasticIPs { get; set; } = false;
        public bool UsePreAllocatedElasticIPs { get; set; } = false;
        public int HostCount { get; set; } = 1;
    }
}