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
    public class IngressRule : SecurityGroupRule
    {
        public Port DestinationPort { get { return Port; } set { Port = value; } }
        public IngressRule(IPeer peer, Port port, params string[] protocols) : base(peer, port, protocols) {}
    }
}
