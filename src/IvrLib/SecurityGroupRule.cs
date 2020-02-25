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
    public abstract class SecurityGroupRule
    {
        public IPeer Peer { get; protected set; }
        public Port Port { get; protected set; }
        public string [] Protocols { get; protected set; }
        public string Description { get; set; }
        public bool RemoteRule { get; set; } = false;
        public SecurityGroupRule(IPeer peer, Port port, params string[] protocols)
        {
            Peer = peer;
            Port = port;
            Protocols = protocols;
        }
        public SecurityGroupRule WithDescription(string description) { Description = description; return this;}
        public SecurityGroupRule WithRemoteRule(bool remoteRule = true) { RemoteRule = remoteRule; return this;}
    }
}
