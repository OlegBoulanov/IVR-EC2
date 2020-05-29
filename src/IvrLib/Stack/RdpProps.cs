using System;
using System.Collections.Generic;
using System.Linq;

namespace IvrLib
{
    public class RdpProps 
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public IEnumerable<string> UserGroups { get; set; } = new List<string> { "Administrators", "RdpUsers" };
        public IEnumerable<string> Cidrs { get; set; }
    }
}