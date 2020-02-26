using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class PortHelpers
    {
        public static Port ParseRange(string s, Func<int, Port> f1, Func<int, int, Port> f2, int p1 = 0, int p2 = 0)
        {
            var range = s.Split('-', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
            if (1 == range.Length) {
                return p1 == p2 || 0 == p2 ? f1(0 < p1 ? p1 : range[0]) : f2(0 < p1 ? p1 : range[0], p2);
            }
            if (2 == range.Length) {
                return 0 < p1 && p1 == p2 ? f1(0 < p1 ? p1 : range[0]) : f2(0 < p1 ? p1 : range[0], 0 < p2 ? p2 : range[1]);
            }
            throw new FormatException($"Can't parse range '{s}', '<begin>[-<end>]' expected");
        }
        public static Port Parse(string s, int p1 = 0, int p2 = 0)
        {
            var fields = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (1 == fields.Length)
            {
                return ParseRange(fields[0], (p) => Port.Tcp(p), (b, e) => Port.TcpRange(b, e), p1, p2);
            }
            else if (2 <= fields.Length)
            {
                switch (fields[0].ToUpper())
                {
                    case "UDP":
                        if(3 == fields.Length && "ALL" == fields[1].ToUpper() && "PORTS" == fields[2].ToUpper()) 
                            return Port.AllUdp();
                        return ParseRange(fields[1], (p) => Port.Udp(p), (b, e) => Port.UdpRange(b, e), p1, p2);
                    case "ICMP":
                        if(3 == fields.Length && "TYPE" == fields[1].ToUpper()) 
                            return Port.IcmpType(0 < p1 ? p1 : int.Parse(fields[2]));
                        if(5 == fields.Length && "TYPE" == fields[1].ToUpper() && "CODE" == fields[3].ToUpper()) 
                            return Port.IcmpTypeAndCode(0 < p1 ? p1 : int.Parse(fields[2]), 0 < p2 ? p2 : int.Parse(fields[4]));
                        break;
                    case "ALL":
                        switch (fields[1].ToUpper())
                        {
                            case "TRAFFIC": return Port.AllTraffic();
                            case "PORTS": return Port.AllTcp();
                        }
                        break;
                }
            }
            throw new FormatException($"Can't parse Port: '{s}'");
        }
        public static Port Clone(Port port, int startPort = 0, int endPort = 0)
        {
            return Parse(port.ToString(), startPort, endPort);
        }
    }
}
