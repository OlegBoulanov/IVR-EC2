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
    public class IngressRuleTemplate : SecurityGroupRule
    {
        public IngressRuleTemplate(IPeer peer, string description = null, bool remote = false) : base(peer, null, description, remote) {}
    }
}
