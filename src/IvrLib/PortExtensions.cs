using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public static class PortExtensions
    {
        public static Port Clone(this Port port, int startPort = 0, int endPort = 0)
        {
            var ps = PortSpec.Parse(port.ToString());
            if (0 < startPort)
            {
                ps.StartPort = startPort;
                ps.EndPort = endPort;
            }
            switch (ps.Protocol)
            {
                case "UDP": return 0 < ps.StartPort ? ps.StartPort < ps.EndPort ? Port.UdpRange(ps.StartPort, ps.EndPort) : Port.Udp(ps.StartPort) : Port.AllUdp();
                case "TCP": return 0 < ps.StartPort ? ps.StartPort < ps.EndPort ? Port.TcpRange(ps.StartPort, ps.EndPort) : Port.Tcp(ps.StartPort) : Port.AllTcp();
            }
            throw new NotImplementedException($"Can't clone {port}");
        }
    }
}