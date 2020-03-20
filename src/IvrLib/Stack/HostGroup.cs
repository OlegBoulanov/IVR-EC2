using System;
using System.Collections.Generic;
using System.Linq;

namespace IvrLib
{
    public class HostGroup
    {
        public string GroupName { get; set; }
        public string InstallFrom { get; set; }
        public bool UseElasticIP { get; set; } = false;
        public int HostCount { get; set; } = 1;
    }
}