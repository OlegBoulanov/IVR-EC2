using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;


namespace Ivr
{
    public class IvrStackProps : StackProps
    {
        public string KeyName { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string[] UserGroups { get; set; } = new string[] { "Administrators", "Remote Desktop Users" };
        public Dictionary<string, int> IngressRules { get; set; }
    }
}