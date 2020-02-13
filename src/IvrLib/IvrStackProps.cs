using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;

namespace IvrLib
{
    public class IvrStackProps : StackProps
    {
        //public string KeyName { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public IEnumerable<string> UserGroups { get; set; } = new List<string> { "Administrators", "Remote Desktop Users" };
        public IDictionary<string, int> IngressRules { get; set; }
        public IEnumerable<string> EC2Users { get; set; }
        public string s3i_args { get; set; }    // = "https://raw.githubusercontent.com/OlegBoulanov/s3i/develop/Examples/Config.ini --verbose";
    }
}