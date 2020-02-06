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
        public Dictionary<string, int> IngressRules { get; set; }
    }
}