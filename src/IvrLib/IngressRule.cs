using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Amazon.CDK;
using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class IngressRule
    {
        public IPeer Peer { get; set; }
        public Port Port { get; set; }
        public string Description { get; set; }
        public bool RemoteRule { get; set; }
    }
}
