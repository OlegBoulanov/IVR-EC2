using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib.Utils
{
    public class PortSpec
    {

        public string Protocol { get; set; }
        public int StartPort { get; set; }
        public int EndPort { get; set; }
        public static PortSpec Parse(string s)
        {
            var prot_ports = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (2 == prot_ports.Length)
            {
                var ports = prot_ports[1].Split('-').Select(p => int.Parse(p)).ToArray();
                return new PortSpec { Protocol = prot_ports[0].ToUpper(), StartPort = 0 < ports.Length ? ports[0] : 0, EndPort = 1 < ports.Length ? ports[1] : 0, };
            }
            throw new FormatException($"Can't parse PortSpec: '{s}', '<protocol> <port>[-<port=0>]' expected");
        }
        public override string ToString()
        {
            return $"{Protocol} {StartPort}" + $"{(StartPort < EndPort ? $"-{EndPort}" : "")}";
        }
    }
}
