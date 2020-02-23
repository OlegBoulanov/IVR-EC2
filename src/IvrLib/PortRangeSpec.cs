using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib
{
    public class PortRangeSpec
    {
        public int Begin { get; set; }
        public int End { get; set; }
        public string Description { get; set; }
        public static PortRangeSpec Parse(string s)
        {
            var rangeDescSpec = s.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (2 < rangeDescSpec.Length) throw new ArgumentNullException($"Invalid port range spec: '{s}', too many colon separated fields");
            var description = rangeDescSpec.Length < 2 ? null : rangeDescSpec[1];
            var portRangeSpec = rangeDescSpec[0].Split('-', StringSplitOptions.RemoveEmptyEntries);
            if(0 < portRangeSpec.Length && int.TryParse(portRangeSpec[0], out var begin)) {
                if(1 == portRangeSpec.Length) 
                    return new PortRangeSpec { Begin = begin, End = begin, Description = description, };
                if(2 == portRangeSpec.Length && int.TryParse(portRangeSpec[1], out var end))
                    return new PortRangeSpec { Begin = begin, End = end, Description = description, };
            }
            throw new FormatException($"Invalid port range spec: '{portRangeSpec}', one or two (dash separated) ports epected");
        }
    }
}