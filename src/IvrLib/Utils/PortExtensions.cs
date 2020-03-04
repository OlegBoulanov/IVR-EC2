using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CDK.AWS.EC2;

namespace IvrLib.Utils
{
    public static class PortExtensions
    {
        public static Port Clone(this Port port, int startPort = 0, int endPort = 0)
        {
            return PortHelpers.Clone(port, startPort, endPort);
        }
    }
}