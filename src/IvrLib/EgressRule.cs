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
    public class EgressRule : SecurityGroupRule
    {
        public Port SourcePort { get { return Port; } set { Port = value; } }
        public EgressRule(IPeer peer, Port port, string description = null, bool remote = false) : base(peer, port, description, remote) {}
    }
}