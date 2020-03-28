using System;
using System.Collections.Generic;
using System.Linq;

namespace IvrLib
{
    public class HostGroup
    {
        public IvrInstanceProps InstanceProps { get; set; } = new IvrInstanceProps();
        public IEnumerable<string> DownloadAndInstall { get; set; }
        public string Install { get; set; }
        public bool UseElasticIP { get; set; } = false;
        public int HostCount { get; set; } = 1;
    }
}