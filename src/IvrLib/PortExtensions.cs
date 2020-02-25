using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public static class PortExtensions
    {
        public static (string prot, int begin, int end) Split(this Port port)
        {
            // Port.ToString(): "UDP 2000-3000"
            var prot_ports = port.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (2 == prot_ports.Length)
            {
                var ports = prot_ports[1].Split('-').Select(p => int.Parse(p)).ToArray();
                return (prot_ports[0].ToUpper(), 0 < ports.Length ? ports[0] : 0, 1 < ports.Length ? ports[1] : 0);
            }
            throw new ArgumentException($"Can't split port '{port}'");
        }
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